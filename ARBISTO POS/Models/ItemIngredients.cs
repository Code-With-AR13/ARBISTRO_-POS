using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class ItemIngredients
    {
        [Key]
        public int Id { get; set; }   // ✅ AUTO IDENTITY (MAIN FIX)

        public int ItemId { get; set; }
        public int IngredientId { get; set; }

        [Required]
        public decimal ConsumptionQty { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal AvailableQty { get; set; }

        public Items Item { get; set; }
        public Ingredients Ingredient { get; set; }

    }
}
