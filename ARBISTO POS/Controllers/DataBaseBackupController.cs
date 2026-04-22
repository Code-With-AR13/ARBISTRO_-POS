using ARBISTO_POS.Attributes;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace ARBISTO_POS.Controllers
{
    public class BackupSettings
    {
        public string DatabaseName { get; set; }
        public string BackupFolder { get; set; }
    }
    [Permission("Database Backup")]
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
        // GET: Generate -> create DB backup + insert row + download
        public async Task<IActionResult> Generate()
        {
            try
            {
                // 1) File name + path
                var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{_backupSettings.DatabaseName}_{timeStamp}.bak";
                var fullPath = Path.Combine(_backupSettings.BackupFolder, fileName);

                if (!Directory.Exists(_backupSettings.BackupFolder))
                    Directory.CreateDirectory(_backupSettings.BackupFolder);

                // 2) SQL BACKUP command
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

                // 3) File size
                var fileInfo = new FileInfo(fullPath);
                var sizeString = $"{Math.Round(fileInfo.Length / 1024m / 1024m, 2)} MB";

                // 4) Row add in table
                var entity = new DataBaseBackup
                {
                    File_Name = fileName,
                    File_Size = sizeString,
                    Discription = "Manual backup (downloaded)",
                    Backup_Date = DateTime.Now
                };

                _context.DataBaseBackups.Add(entity);
                await _context.SaveChangesAsync();

                // Optional message (Index pe agar wapas aaye to)
                TempData["SuccessMessage"] = "Database backup generated successfully.";

                // 5) Ab direct download par redirect
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DataBaseBackups
                .Select(x => new
                {
                    backupNumber = x.BackupNumber,
                    file_Name = x.File_Name,
                    file_Size = x.File_Size,
                    backup_Date = x.Backup_Date.ToString("yyyy-MM-dd HH:mm"),
                    discription = x.Discription
                })
                .ToListAsync();

            return Json(data);
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
