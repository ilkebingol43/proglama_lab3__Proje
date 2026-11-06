using System.Text.RegularExpressions;

namespace Lab3_proje.Services.Validation
{
    public class TelefonKontrolcu : KuralKontrolcu
    {



        public TelefonKontrolcu()
        {
            KuralAdi = "Telefon numarası format kontorlü";
        }

        public override bool KontrolEt(object veri, out string hataMesaji)
        {
            hataMesaji = string.Empty;
            string telefon = veri?.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(telefon))
            {
                hataMesaji = "telefon numarası bos bırakıkalmaz";
                return false;
            }

            string pattern = @"^\+90\s\d{3}\s\d{3}\s\d{2}\s\d{2}$";

            if(!Regex.IsMatch(telefon, pattern))
            {
                hataMesaji = "telefon formatı geçersiz";
                return false;
            }

            return true;
        }

    }
}
