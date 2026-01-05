using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class ExpenseType
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ExpenseType name is required")]
        [StringLength(1000, ErrorMessage = "ExpenseType name cannot exceed 1000 characters")]
        [Display(Name = "PaymentMethods Name")]
        public string ExpenseName { get; set; }

        [Display(Name = "Description")]
        public string? ExpDescription { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
