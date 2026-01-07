using ARBISTO_POS.Models;

namespace ARBISTO_POS.ViewModels
{
    public class OverallReportsViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }
        public string ChefName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    // Main ViewModel (filters + totals + items)
    public class OverAllReportVm
    {
        // Filters
        public string OrderType { get; set; }
        public int? ChefId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Dropdowns
        public List<string> OrderTypes { get; set; } = new() { "Dine In", "Pickup Point", "Delivery" };
        public List<Employees> Chefs { get; set; } = new();

        // Result items
        public List<OverallReportsViewModel> Items { get; set; } = new();

        // === 6 Summary totals ===
        public decimal TotalSaleAmount => Items.Sum(x => x.SubTotal);
        public decimal TotalCostAmount { get; set; }       // alag se calculate karna padega (cost info items se)
        public decimal TotalDiscountAmount => Items.Sum(x => x.DiscountAmount);
        public decimal TotalTaxAmount => Items.Sum(x => x.TaxAmount);
        public decimal TotalPayableAmount => Items.Sum(x => x.GrandTotal);
        public decimal TotalProfitAmount => TotalSaleAmount - TotalCostAmount - TotalDiscountAmount;
    }
}

