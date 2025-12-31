namespace ARBISTO_POS.ViewModels
{
    public class HoldOrderViewModel
    {
        // Order Info
        public string? OrderNumber { get; set; }
        public string? OrderType { get; set; }

        // Customer Info
        public int? CustomerId { get; set; }

        // Order Type Specific
        public int? TableId { get; set; }
        public int? PickUpId { get; set; }
        public string? DelivaryAddress { get; set; }

        // Payment
        public int? PaymentId { get; set; }
        public string? PaymentStatus { get; set; } = "Unpaid";

        // Amounts
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? GrandTotal { get; set; }

        // Items
        public List<HoldOrderItemViewModel> Items { get; set; } = new();
    }

    public class HoldOrderItemViewModel
    {
        public int? ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public string? ItemImage { get; set; }
    }
}

