using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class HeldOrders
    {
        [Key]
        public int OrderId { get; set; }

        // ================= Order Info =================
        [Required]
        public string? OrderNumber { get; set; }   // ORD-1001

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        // ================= Order Type =================
        [Required]
        public string? OrderType { get; set; }     // DineIn, TakeAway, Delivery

        // ================= Status =================
        public string? OrderStatus { get; set; }   // Pending, Preparing, Ready

        // ================= Customer =================
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customers Customer { get; set; }

        // ================= Table (Dine-In) =================
        public int? TableId { get; set; }

        [ForeignKey(nameof(TableId))]
        public ServiceTables Table { get; set; }

        // ================= Pickup (TakeAway) =================
        public int? PickUpId { get; set; }

        [ForeignKey(nameof(PickUpId))]
        public PickPoints PickUp { get; set; }
        // ================= Pickup (TakeAway) =================        
        public string? DelivaryAddress { get; set; }


        // ================= Payment =================
        public int? PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public PaymentMethods Payment { get; set; }

        public string? PaymentStatus { get; set; } = "Unpaid";

        // ================= Amounts =================
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? GrandTotal { get; set; }

        // ================= Staff / Chef =================
        public int? ChefId { get; set; }

        [ForeignKey(nameof(ChefId))]
        public Employees Chef { get; set; }       
        // ================= Navigation =================
        public ICollection<HeldOrdersItem>? HeldOrderItems { get; set; }
    }
}
