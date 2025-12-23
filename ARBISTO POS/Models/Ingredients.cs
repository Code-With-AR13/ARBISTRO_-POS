using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class Ingredients
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150)]
        public string Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be 0 or greater")]
        public decimal Cost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater")]
        public decimal Price { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [Display(Name = "Available Quantity")]
        [Range(0, double.MaxValue, ErrorMessage = "Available Quantity must be 0 or greater")]
        public decimal AvailableQuantity { get; set; }
        public decimal ConsumptionQty { get; set; }

        [Display(Name = "Quantity alert")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity alert must be 0 or greater")]
        public decimal QuantityAlert { get; set; }
        [Display(Name = "Category Image")]
        public string? CateImage { get; set; }

        // This property is not mapped to database - only for file upload
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
