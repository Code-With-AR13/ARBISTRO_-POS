using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class HeldOrdersItem
    {
        [Key]
        public int OrderItemId { get; set; }

        // ================= Order =================       
        public int? OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public HeldOrders HeldOrder { get; set; }

        // ================= Customer =================
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customers Customer { get; set; }

        // ================= Item =================
        [Required]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Items Item { get; set; }

        // ================= Item Snapshot =================
        public string? ItemName { get; set; }   // order time snapshot
        public decimal? Price { get; set; }

        // ================= Qty & Total =================
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public string? ItemImage { get; set; }
    }
}
