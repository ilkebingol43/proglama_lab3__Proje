using System;
using System.Text.RegularExpressions;

namespace Lab3_proje.Services.Validation
{
    public class OgretimYiliKontrolcu : KuralKontrolcu
    {
        public OgretimYiliKontrolcu()
        {
            KuralAdi = "Öğretim Yılı Formatı Kontrolü";
        }

        public override bool KontrolEt(object veri, out string hataMesaji)
        {
            hataMesaji = string.Empty;

            if (veri is not string ogretimYili || string.IsNullOrWhiteSpace(ogretimYili))
            {
                hataMesaji = "Öğretim yılı boş bırakılamaz.";
                return false;
            }

            var eslesme = Regex.Match(ogretimYili.Trim(), @"^(\d{4})-(\d{4})$");
            if (!eslesme.Success)
            {
                hataMesaji = "Öğretim yılı '2025-2026' şeklinde ardışık iki yıldan oluşmalıdır.";
                return false;
            }

            int baslangicYili = int.Parse(eslesme.Groups[1].Value);
            int bitisYili = int.Parse(eslesme.Groups[2].Value);

            if (bitisYili != baslangicYili + 1)
            {
                hataMesaji = $"Öğretim yılı ardışık yıllardan oluşmalıdır (Örnek: {baslangicYili}-{baslangicYili + 1}).";
                return false;
            }

            return true;
        }
    }
}