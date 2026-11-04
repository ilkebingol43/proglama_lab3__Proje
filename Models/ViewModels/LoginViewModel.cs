using System.ComponentModel.DataAnnotations;

namespace Lab3_proje.Models.ViewModels
{
    public class LoginViewModel
    {

    
        [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta formatı giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

    }
}
