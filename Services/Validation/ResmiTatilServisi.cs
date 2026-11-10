using System;
using System.Collections.Generic;

namespace Lab3_proje.Services.Validation
{
    public static class ResmiTatilServisi
    {
        private static readonly Dictionary<DateTime, string> DiniVeDegiskenTatiller = new()
        {
            // 2024
            { new DateTime(2024, 4, 9), "Ramazan Bayramı Arifesi" },
            { new DateTime(2024, 4, 10), "Ramazan Bayramı 1. Gün" },
            { new DateTime(2024, 4, 11), "Ramazan Bayramı 2. Gün" },
            { new DateTime(2024, 4, 12), "Ramazan Bayramı 3. Gün" },
            { new DateTime(2024, 6, 15), "Kurban Bayramı Arifesi" },
            { new DateTime(2024, 6, 16), "Kurban Bayramı 1. Gün" },
            { new DateTime(2024, 6, 17), "Kurban Bayramı 2. Gün" },
            { new DateTime(2024, 6, 18), "Kurban Bayramı 3. Gün" },
            { new DateTime(2024, 6, 19), "Kurban Bayramı 4. Gün" },

            // 2025
            { new DateTime(2025, 3, 29), "Ramazan Bayramı Arifesi" },
            { new DateTime(2025, 3, 30), "Ramazan Bayramı 1. Gün" },
            { new DateTime(2025, 3, 31), "Ramazan Bayramı 2. Gün" },
            { new DateTime(2025, 4, 1), "Ramazan Bayramı 3. Gün" },
            { new DateTime(2025, 6, 5), "Kurban Bayramı Arifesi" },
            { new DateTime(2025, 6, 6), "Kurban Bayramı 1. Gün" },
            { new DateTime(2025, 6, 7), "Kurban Bayramı 2. Gün" },
            { new DateTime(2025, 6, 8), "Kurban Bayramı 3. Gün" },
            { new DateTime(2025, 6, 9), "Kurban Bayramı 4. Gün" },

            // 2026
            { new DateTime(2026, 3, 19), "Ramazan Bayramı Arifesi" },
            { new DateTime(2026, 3, 20), "Ramazan Bayramı 1. Gün" },
            { new DateTime(2026, 3, 21), "Ramazan Bayramı 2. Gün" },
            { new DateTime(2026, 3, 22), "Ramazan Bayramı 3. Gün" },
            { new DateTime(2026, 5, 26), "Kurban Bayramı Arifesi" },
            { new DateTime(2026, 5, 27), "Kurban Bayramı 1. Gün" },
            { new DateTime(2026, 5, 28), "Kurban Bayramı 2. Gün" },
            { new DateTime(2026, 5, 29), "Kurban Bayramı 3. Gün" },
            { new DateTime(2026, 5, 30), "Kurban Bayramı 4. Gün" },

            // 2027
            { new DateTime(2027, 3, 9), "Ramazan Bayramı 1. Gün" },
            { new DateTime(2027, 3, 10), "Ramazan Bayramı 2. Gün" },
            { new DateTime(2027, 3, 11), "Ramazan Bayramı 3. Gün" },
            { new DateTime(2027, 5, 16), "Kurban Bayramı 1. Gün" },
            { new DateTime(2027, 5, 17), "Kurban Bayramı 2. Gün" },
            { new DateTime(2027, 5, 18), "Kurban Bayramı 3. Gün" }
        };

        // Verilen tarihin resmi veya ulusal tatil olup olmadığını doğrular
        public static bool TatilMi(DateTime tarih, out string? tatilAdi)
        {
            var dateOnly = tarih.Date;

            // Her yıl sabit olan ulusal bayramlar
            if (dateOnly.Month == 1 && dateOnly.Day == 1) { tatilAdi = "Yılbaşı"; return true; }
            if (dateOnly.Month == 4 && dateOnly.Day == 23) { tatilAdi = "Ulusal Egemenlik ve Çocuk Bayramı"; return true; }
            if (dateOnly.Month == 5 && dateOnly.Day == 1) { tatilAdi = "Emek ve Dayanışma Günü"; return true; }
            if (dateOnly.Month == 5 && dateOnly.Day == 19) { tatilAdi = "Atatürk'ü Anma, Gençlik ve Spor Bayramı"; return true; }
            if (dateOnly.Month == 7 && dateOnly.Day == 15) { tatilAdi = "15 Temmuz Demokrasi ve Milli Birlik Günü"; return true; }
            if (dateOnly.Month == 8 && dateOnly.Day == 30) { tatilAdi = "Zafer Bayramı"; return true; }
            if (dateOnly.Month == 10 && dateOnly.Day == 29) { tatilAdi = "Cumhuriyet Bayramı"; return true; }

            // Dini ve değişken takvimli bayramlar
            if (DiniVeDegiskenTatiller.TryGetValue(dateOnly, out var ad))
            {
                tatilAdi = ad;
                return true;
            }

            tatilAdi = null;
            return false;
        }
    }
}