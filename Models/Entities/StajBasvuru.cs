using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3_proje.Models.Entities
{
    public class StajBasvuru
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public string DosyaAdi { get; set; } = string.Empty;
        [Required]
        public string DosyaYolu { get; set; } = string.Empty;
        public string? DosyaUzantisi { get; set; }
        public DateTime YuklemeTarihi { get; set; } = DateTime.Now;
        public string Statu { get; set; } = "yüklendi";
        public string? raporMesajı { get; set; }

        public DateTime StajBaslangicTarihi { get; set; }

        public DateTime StajBitisTarihi { get; set; }

        public string SecilenHaftalikGunler { get; set; } = string.Empty;
        public int HesaplanmısStajGunSayısı { get; set; } = 0;
        public bool DuzenlenebilirMi => Statu != "Tamamlandi";
    }
}
