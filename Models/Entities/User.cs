using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace Lab3_proje.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "İsim Girmek Zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Minimum 2, Maximum 50 Karakter Girebilirsiniz")]
        public string Name { get; set; } = string.Empty;
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Minimum 5, Maximum 20 Karakter Girebilirsiniz")]
        public string UserName { get; set; } = string.Empty;
        [NotMapped] // Veritabanına kaydedilmez, formdan geçici almak içindir
        public string Password { get; set; } = string.Empty;
        // Veritabanına sadece hash hali kaydedilir (private set)
        public string PasswordHash { get; private set; } = string.Empty;
        [Required(ErrorMessage = "Email Girmek Zorunludur")]
        public string Email { get; set; } = string.Empty;
        // NYP Kapsülleme (Encapsulation): Sadece metotlarla değişebilir
        public int FailedLoginAttempts { get; private set; } = 0;
        public bool KilitlendiMi { get; set; } = false;
        public DateTime? LockoutEndTime { get; set; }
        public string Role { get; set; } = "ROLE_STUDENT";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Şifreyi BCrypt ile güvenli hash'e çeviren metot
        public void SifreyiHashle(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password); // şifreyi hashladik
        }
        // İsimdeki fazla boşlukları temizleyen metot
        public string BoslukTemizle(string name)
        {
            this.Name = Regex.Replace(name.Trim(), @"\s+", " ");
            return this.Name;
        }
        // 5 hatalı denemede hesabı kilitleyen kural
        public void GirisLoglama()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
            {
                KilitlendiMi = true;
                LockoutEndTime = DateTime.Now.AddMinutes(15); // 15 dakika kilitli kalır
            }
        }
        // Başarılı girişte sayacı ve kilidi sıfırlayan metot
        public void GirisSayisiSifirlama()
        {
            FailedLoginAttempts = 0;
            KilitlendiMi = false;
            LockoutEndTime = null;
        }
    }
}
