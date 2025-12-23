using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class Employees
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public string? Address { get; set; }                  
        
        [StringLength(20)]
        public string? Shift { get; set; }

        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;     

        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? Discription { get; set; }

        [Display(Name = "Employee Image")]
        public string? EmpImage { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
