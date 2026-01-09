namespace ARBISTO_POS.Models
{
    public class IngredientTransaction
    {
        public int Id { get; set; }
        public int IngredientId { get; set; }
        public decimal QuantityUsed { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string TransactionType { get; set; } // "Used", "Added", "Adjusted"
        public string? ReferenceType { get; set; } // "Modifier", "Item", "Purchase"
        public int? ReferenceId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }

        public virtual Ingredients Ingredient { get; set; }
    }
}
