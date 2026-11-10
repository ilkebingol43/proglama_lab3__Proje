using System;

namespace Lab3_proje.Services.Validation
{
    public class MuhendisBilgiKontrolcu : KuralKontrolcu
    {
        public MuhendisBilgiKontrolcu()
        {
            KuralAdi = "Mühendis Sayısı ve Mezuniyet Bölümü Kontrolü";
        }

        public override bool KontrolEt(object veri, out string hataMesaji)
        {
            hataMesaji = string.Empty;

            if (veri is not ValueTuple<int, string?> bilgiler)
            {
                hataMesaji = "Geçersiz mühendis bilgisi girdisi!";
                return false;
            }

            int muhendisSayisi = bilgiler.Item1;
            string? mezuniyetBolumu = bilgiler.Item2;

            if (muhendisSayisi < 1)
            {
                hataMesaji = "Staj yapılacak kurumda çalışan mühendis sayısı en az 1 olmalıdır.";
                return false;
            }

            if (muhendisSayisi < 5 && string.IsNullOrWhiteSpace(mezuniyetBolumu))
            {
                hataMesaji = "Kurumda çalışan mühendis sayısı 5'ten az olduğunda, mühendisin mezun olduğu lisans bölümü zorunludur.";
                return false;
            }

            return true;
        }
    }
}