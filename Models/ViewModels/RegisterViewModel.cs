using System.ComponentModel.DataAnnotations;

namespace Lab3_proje.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad ve Soyad girmek zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı girmek zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi girmek zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta formatı giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre girmek zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Girdiğiniz şifreler birbiriyle eşleşmiyor!")]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;


        [Display(Name = "Yönetici Özel Kodu")]
        public string? YoneticiKodu { get; set; }
    }
}