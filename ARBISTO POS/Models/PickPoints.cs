using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.Models
{
    public class PickPoints
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "PaymentMethods name is required")]
        [StringLength(100, ErrorMessage = "PaymentMethods name cannot exceed 100 characters")]
        [Display(Name = "PaymentMethods Name")]
        public string PicTittle { get; set; }

        [Display(Name = "Description")]
        public string? PicDescription { get; set; }
        public string PersonName { get; set; }
        public string PhoneNo { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
