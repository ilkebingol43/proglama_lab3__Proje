using System; 


namespace Lab3_proje.Services.Validation
{
   
    public class StajTarihKontrolcu : KuralKontrolcu
    {
        public StajTarihKontrolcu()
        {
            KuralAdi = "Staj Teslim Tarihi Kontrolü (30 Gün Kuralı)";
        }

        
        public override bool KontrolEt(object veri, out string hataMesaji)
        {
            hataMesaji = string.Empty;

            if (veri is not DateTime stajBaslangicTarihi)
            {
                hataMesaji = "Geçersiz staj başlangıç tarihi girdisi!";
                return false;
            }

            DateTime bugun = DateTime.Today;
            TimeSpan fark = stajBaslangicTarihi.Date - bugun;

            
            if (fark.TotalDays < 30)
            {
                DateTime onerilenSonTarih = stajBaslangicTarihi.AddDays(-30);
                hataMesaji = $"Belge, staj başlangıç tarihinden en az 30 gün önce teslim edilmelidir! " +
                             $"Bu staj için en geç teslim tarihi {onerilenSonTarih:dd.MM.yyyy} olmalıydı.";
                return false;
            }

            return true;
        }
    }
}