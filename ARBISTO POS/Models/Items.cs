using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ARBISTO_POS.Models
{
    public class Items
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100)]
        public string ItemName { get; set; }

        // ✅ Category FK (ONE Item → ONE Category)
        [Required(ErrorMessage = "Category is required")]
        [ForeignKey("FoodCategory")]
        public int FoodCategoryId { get; set; }
        public virtual FoodCategories FoodCategory { get; set; }

        public string ItemDiscription { get; set; }

        [Display(Name = "Category Image")]
        public string? CateImage { get; set; }

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
        public ICollection<ItemIngredients> ItemIngredients { get; set; }
            = new List<ItemIngredients>();
    }
}
