using ARBISTO_POS.Models;

namespace ARBISTO_POS.ViewModels
{
    public class ExpenseReportViewModel
    {       
            public DateTime CreatedDate { get; set; }
            public string ExpenseName { get; set; }
            public string ExpenseTypeName { get; set; }
            public int ExpenseAmount { get; set; }
    }

        public class ExpenseReportVm
        {
            // Filters
            public int? ExpenseTypeId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }

            // Dropdown ke liye
            public List<ExpenseType> ExpenseTypes { get; set; } = new();

            // Result items
            public List<ExpenseReportViewModel> Items { get; set; } = new();

            // Total card ke liye
            public int TotalExpense => Items.Sum(x => x.ExpenseAmount);
        }

    }
