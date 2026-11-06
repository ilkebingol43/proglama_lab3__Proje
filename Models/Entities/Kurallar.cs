using System.Linq;
using System.Text.RegularExpressions;

namespace Lab3_proje.Models.Entities
{
    public class Kurallar
    {
        // İsim Kontrolü: Boş olamaz, sadece harf ve boşluk içerebilir, 2-50 karakter arası olmalı
        public bool IsimKontrol(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Name) || !user.Name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                return false;
            }

            if (user.Name.Length < 2 || user.Name.Length > 50)
            {
                return false;
            }

            return true;
        }

        // sifre kontrolü: 8-12 karakter arası, en az 1 büyük harf ve 1 özel karakter içermeli
        public bool PasswordKontrol(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 8 || user.Password.Length > 12)
            {
                return false;
            }

            bool buyukHarfVarMi = user.Password.Any(char.IsUpper);
            bool ozelKarakterVarMi = user.Password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

            if (!buyukHarfVarMi || !ozelKarakterVarMi)
            {
                return false;
            }

            return true;
        }

        // eposta kontrolü: Regex ile ad@alanadi.uzanti format kontrolü
        public bool EpostaKontrol(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(user.Email, emailPattern);
        }
    }
}