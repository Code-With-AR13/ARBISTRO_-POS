using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class DataBaseBackup
    {
        [Key]
        public int BackupNumber { get; set; }
        public string File_Name { get; set; }
        public string File_Size { get; set; }
        public string? Discription { get; set; }
        public DateTime Backup_Date { get; set; } = DateTime.UtcNow;
    }
}
