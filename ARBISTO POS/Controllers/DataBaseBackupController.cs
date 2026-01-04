using System.Data;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ARBISTO_POS.Controllers
{
    public class BackupSettings
    {
        public string DatabaseName { get; set; }
        public string BackupFolder { get; set; }
    }

    public class DataBaseBackupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly BackupSettings _backupSettings;

        public DataBaseBackupController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IOptions<BackupSettings> backupOptions)
        {
            _context = context;
            _configuration = configuration;
            _backupSettings = backupOptions.Value;
        }

        // GET: listing
        public async Task<IActionResult> Index()
        {
            var backups = await _context.DataBaseBackups
                                        .OrderByDescending(x => x.Backup_Date)
                                        .ToListAsync();
            return View(backups);
        }
        // GET: Generate -> create DB backup + insert/replace row + download
        public async Task<IActionResult> Generate()
        {
            try
            {
                // 0) Purane backups delete karo (optional: sirf 1 row rakhni)
                var oldBackups = _context.DataBaseBackups.ToList();
                foreach (var b in oldBackups)
                {
                    var oldPath = Path.Combine(_backupSettings.BackupFolder, b.File_Name);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                _context.DataBaseBackups.RemoveRange(oldBackups);
                await _context.SaveChangesAsync();

                // 1) Ensure folder
                var backupFolder = _backupSettings.BackupFolder;
                if (string.IsNullOrWhiteSpace(backupFolder))
                    backupFolder = Path.Combine(AppContext.BaseDirectory, "DBBackups");

                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                // 2) Fixed file name (hamesha same, overwrite)
                var fileName = $"{_backupSettings.DatabaseName}_Latest.bak";
                var fullPath = Path.Combine(backupFolder, fileName);

                // 3) SQL BACKUP command - WITH INIT se same file overwrite hogi [web:108][web:110][web:111]
                var backupSql =
                    $"BACKUP DATABASE [{_backupSettings.DatabaseName}] " +
                    $"TO DISK = @path WITH FORMAT, INIT;";

                var connString = _configuration.GetConnectionString("DefaultConnection");

                using (var con = new SqlConnection(connString))
                using (var cmd = new SqlCommand(backupSql, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@path", fullPath);
                    cmd.CommandTimeout = 300;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // 4) File size
                var fileInfo = new FileInfo(fullPath);
                var sizeString = $"{Math.Round(fileInfo.Length / 1024m / 1024m, 2)} MB";

                // 5) New single row insert
                var entity = new DataBaseBackup
                {
                    File_Name = fileName,
                    File_Size = sizeString,
                    Discription = "Latest backup",
                    Backup_Date = DateTime.Now
                };

                _context.DataBaseBackups.Add(entity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Database backup generated successfully.";

                // 6) Direct download
                return RedirectToAction(nameof(Download), new { id = entity.BackupNumber });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Backup failed: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }




        // GET: Download backup file (Save As dialog)
        public IActionResult Download(int id)
        {
            var entity = _context.DataBaseBackups
                                 .FirstOrDefault(x => x.BackupNumber == id);
            if (entity == null)
                return NotFound();

            var fullPath = Path.Combine(_backupSettings.BackupFolder, entity.File_Name);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(fullPath);
            return File(bytes, "application/octet-stream", entity.File_Name);
        }


        // POST: Delete (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.DataBaseBackups
                                       .FirstOrDefaultAsync(x => x.BackupNumber == id);
            if (entity == null)
                return Json(new { success = false, message = "Backup not found." });

            // delete file from disk as well
            try
            {
                var fullPath = Path.Combine(_backupSettings.BackupFolder, entity.File_Name);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch { }

            _context.DataBaseBackups.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Backup deleted successfully." });
        }



        // GET: Restore database from backup
        public async Task<IActionResult> Restore(int id)
        {
            // 1) Backup record nikaalo
            var entity = await _context.DataBaseBackups
                                       .FirstOrDefaultAsync(x => x.BackupNumber == id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Backup not found.";
                return RedirectToAction(nameof(Index));
            }

            // 2) .bak file ka full path
            var backupFilePath = Path.Combine(_backupSettings.BackupFolder, entity.File_Name);
            if (!System.IO.File.Exists(backupFilePath))
            {
                TempData["ErrorMessage"] = "Backup file not found on disk.";
                return RedirectToAction(nameof(Index));
            }

            // 3) Connection string ko master DB par point karo
            var defaultConn = _configuration.GetConnectionString("DefaultConnection");
            var builder = new SqlConnectionStringBuilder(defaultConn);
            builder.InitialCatalog = "master";   // IMPORTANT: master
            builder.ConnectTimeout = 60;
            var masterConnString = builder.ConnectionString;

            try
            {
                using (var con = new SqlConnection(masterConnString))
                {
                    await con.OpenAsync();

                    // ---------- STEP 1: SINGLE_USER + kill connections ----------
                    var setSingleUserSql =
                        $"ALTER DATABASE [{_backupSettings.DatabaseName}] " +
                        "SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                    using (var cmd = new SqlCommand(setSingleUserSql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 300;
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // ---------- STEP 2: (optional) logical names, agar chahiye to ----------
                    // Agar tum logicalDataName/logicalLogName use karna chahte ho to yahan
                    // RESTORE FILELISTONLY ka code rakh sakte ho; abhi simple direct restore kar rahe hain.

                    // ---------- STEP 3: Actual RESTORE ----------
                    var restoreSql =
                        $"RESTORE DATABASE [{_backupSettings.DatabaseName}] " +
                        $"FROM DISK = @backupPath " +
                        $"WITH REPLACE, RECOVERY;";

                    using (var cmd = new SqlCommand(restoreSql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 300;
                        cmd.Parameters.AddWithValue("@backupPath", backupFilePath);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // ---------- STEP 4: Wapis MULTI_USER ----------
                    var setMultiUserSql =
                        $"ALTER DATABASE [{_backupSettings.DatabaseName}] " +
                        "SET MULTI_USER WITH ROLLBACK IMMEDIATE;";

                    using (var cmd = new SqlCommand(setMultiUserSql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 300;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                TempData["SuccessMessage"] = "Database restored successfully from backup.";
            }
            catch (Exception ex)
            {
                // Safety: agar beech me fail ho jaye to bhi DB ko MULTI_USER try karo
                try
                {
                    using (var con = new SqlConnection(masterConnString))
                    {
                        await con.OpenAsync();
                        var fixSql =
                            $"ALTER DATABASE [{_backupSettings.DatabaseName}] " +
                            "SET MULTI_USER WITH ROLLBACK IMMEDIATE;";
                        using (var cmd = new SqlCommand(fixSql, con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 300;
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch { }

                TempData["ErrorMessage"] = "Restore failed: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }      

    }
}
