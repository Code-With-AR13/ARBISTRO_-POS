using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class ServiceTables
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ServiceTables name is required")]
        [StringLength(100, ErrorMessage = "ServiceTables name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string TabName { get; set; }

        [Display(Name = "Description")]
        public string? TabDescription { get; set; }

        [Display(Name = "Category Image")]
        public string? TabImage { get; set; }

        // This property is not mapped to database - only for file upload
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
