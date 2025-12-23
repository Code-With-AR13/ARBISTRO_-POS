using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ARBISTO_POS.Models
{
    public class FoodCategories
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CateName { get; set; }    
        
        [Display(Name = "Description")]
        public string Description { get; set; }               

        [Display(Name = "Category Image")]
        public string? CateImage { get; set; }

        // This property is not mapped to database - only for file upload
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
