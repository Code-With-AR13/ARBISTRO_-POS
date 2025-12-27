using ARBISTO_POS.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ARBISTO_POS.ViewModels
{
    public class PosViewModel
    {
        public List<FoodCategories> Categories { get; set; } = new();
        public List<Items> Items { get; set; } = new();

        public List<Employees> Employees { get; set; } = new();
        public List<ServiceTables> Tables { get; set; } = new();
        public List<PickPoints> PickupPoints { get; set; } = new();
        public List<PaymentMethods> PaymentMethods { get; set; } = new();
        // Selected customer
        public int? CustomerId { get; set; }
        public List<Customers> Customers { get; set; } = new();

        public SaleOrderItems OrderItems { get; set; }
        public SaleOrders Order { get; set; } = new SaleOrders();

    }
}
