

namespace Lab3_proje.Services.Validation
{
    public  class TCKimlikKontrolcu : KuralKontrolcu

    {
        public TCKimlikKontrolcu() {
            KuralAdi = "tc/Yabanci  Kimlik No Kontrolu";
        }
        public override bool KontrolEt(object veri, out string hataMesaji)
        {
            hataMesaji = string.Empty;
            string girdi = veri?.ToString() ?? string.Empty;
            string[] parcalar = girdi.Split('|');
            string TcNo = parcalar[0].Trim();
            string uyruk = parcalar.Length > 1 ? parcalar[1].Trim().ToLower() : "TC";


            if (string.IsNullOrWhiteSpace(TcNo))  // boşlukları kontrol eder
            {
                hataMesaji = "Kimlik Numarası boş bırakılamaz";

            }

            if (!TcNo.All(char.IsDigit)) // tcNo rakam mı onları kontrol eder 
            {
                hataMesaji = "Kimlik Numarası Sadece Rakamlardan oluşmaladır";
                return false;

            }
             
            if (TcNo.Length != 11) // tcNo için uzunlugu kontrol eder
            {
                hataMesaji = "Kimlik Numarası 11 Haneli olmalidir";
                return false;
            }


            if (uyruk == "YABANCI") // uyrugu KOntrol eder
            {
                if(!TcNo.StartsWith("99")) //tcNo 99 ile başlamıyorsa hata
                {
                    hataMesaji = "Yabancı Uyruklu ogrencilerin kimlik Numarası 99 ile başlamalıdır";
                    return false;

                }
            }


            else
            {
                if (TcNo.StartsWith("0")) // tcNı 0 ile başlıyamaz
                {
                    hataMesaji = "Tc kimlik Numarası 0 ile başlayamaz";
                }
            }

            return true;
        }
    }
}
