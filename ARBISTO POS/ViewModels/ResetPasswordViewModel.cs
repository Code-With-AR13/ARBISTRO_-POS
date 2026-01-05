using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passward Must be Same")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
