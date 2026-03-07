using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class Modifier
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100)]
        public string ModeName { get; set; }       
        public string? ModeDiscription { get; set; }

        [Display(Name = "Category Image")]
        public string? ModeImage { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Cost must be greater than 0")]
        public int ItemCost { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public int ItemPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        // ✅ Ingredients handled via Many-to-Many
        public ICollection<ModifierIngredients> ModifierIngredients { get; set; }
            = new List<ModifierIngredients>();

    }
}
