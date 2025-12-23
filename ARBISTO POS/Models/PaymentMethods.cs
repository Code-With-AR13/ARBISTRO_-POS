using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class PaymentMethods
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "PaymentMethods name is required")]
        [StringLength(100, ErrorMessage = "PaymentMethods name cannot exceed 100 characters")]
        [Display(Name = "PaymentMethods Name")]
        public string PayName { get; set; }

        [Display(Name = "Description")]
        public string? PayDescription { get; set; }        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
