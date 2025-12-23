using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class Customers
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }
        [Display(Name = "Category Image")]
        public string? CusImage { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
