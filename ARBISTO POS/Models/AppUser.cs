using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARBISTO_POS.Models
{
    public class AppUser
    {    
        public int Id { get; set; }

        [Required, MaxLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = default!;        // unique

        [MaxLength(30)]
        public string? Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;
        [Display(Name = "Category Image")]
        public string? UserImage { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        // These are server-generated – do not validate or bind from the request
        [BindNever, ValidateNever]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [BindNever, ValidateNever]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        // AppUser.cs model mein add karein
        public string? PasswordResetToken { get; set; }
        public DateTime? TokenExpiry { get; set; }


        [Display(Name = "Created (UTC)")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated (UTC)")]
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
