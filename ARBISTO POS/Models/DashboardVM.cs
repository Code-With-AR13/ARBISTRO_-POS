namespace ARBISTO_POS.Models
{
    public class DashboardVM
    {
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int UnpaidOrders { get; set; }
        public decimal TotalExpense { get; set; }

        public List<SaleOrders> LatestOrders { get; set; }
    }
}