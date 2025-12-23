using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class ModifierIngredients
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

        public Modifier Modifiers { get; set; }
        public Ingredients Ingredient { get; set; }

    }
}
