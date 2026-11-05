using System.ComponentModel.DataAnnotations;

namespace Lab3_proje.Models.ViewModels
{
    public class OgrenciLoginViewModel
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adınızı giriniz.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}