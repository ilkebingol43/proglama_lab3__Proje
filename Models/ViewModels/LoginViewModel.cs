using System.ComponentModel.DataAnnotations;

namespace Lab3_proje.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adı veya e-posta adresinizi giriniz.")]
        [Display(Name = "Kullanıcı Adı veya E-posta")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;
    }
}