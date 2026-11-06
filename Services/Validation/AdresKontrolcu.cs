using System.Security;

namespace Lab3_proje.Services.Validation
{
    public class AdresKontrolcu : KuralKontrolcu
    {

        public AdresKontrolcu()
        {
            KuralAdi = "Ikametgah adres format kontrolu";

        }

        public override bool KontrolEt(object veri, out string hataMesaji)
        {
            hataMesaji = string.Empty;
            string adres = veri?.ToString()?.Trim() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(adres)) // boşluk varmı yokmu kontrolu
            {
                hataMesaji = "İkametgah adresi boş bırakılamaz";
                return false;
            }

            string[] zorunluParcalar = { "Mah", "Cad", "Sok", "No" }; /// adreste olması gerekenler
            bool gecerliMi = zorunluParcalar.Any(p => adres.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);// gecerlilik kontrolu


            if(adres.Length<15 || !gecerliMi)
            {
                hataMesaji = "Adres eksik veya format dışı";
                return false;
            }

            return true;

        }




    }
}
