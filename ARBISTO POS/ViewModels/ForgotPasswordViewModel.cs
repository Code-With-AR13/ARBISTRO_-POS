using System.ComponentModel.DataAnnotations;

namespace ARBISTO_POS.ViewModels
{
    public class ForgotPasswordViewModel
    {       
            [Required(ErrorMessage = "Email Address is required")]
            [EmailAddress(ErrorMessage = "Please Enter Valid Email Address")]
            public string Email { get; set; }        
    }
}
