using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class Expenses
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Expense name is required")]
        [StringLength(1000, ErrorMessage = "Expense Name cannot exceed 1000 characters")]
        [Display(Name = "Expense Name")]
        public string ExpenseName { get; set; }
        public int ExpenseAmount { get; set; }
        // ✅ Category FK (ONE Item → ONE Category)
        [Required(ErrorMessage = "ExpenseType is required")]
        [ForeignKey("ExpenseType")]
        public int ExpenseTypeId { get; set; }
        public virtual ExpenseType ExpenseType { get; set; }

        [Display(Name = "Description")]
        public string? ExpDescription { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
