using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab3_proje.Models.Entities
{
    public class OgrenciDetay : User // kalıtım
    {

       
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public DateTime DogumTarihi { get; set; } 

        public string TelefonNumarası { get; set; } = string.Empty;
        public string OkuduguBolum { get; set; } = string.Empty;


        [Required]
        [StringLength(11)]


        public string TCKimlik { get; set; } = string.Empty;

        public string BelgeTeslimYili { get; set; } = "2025-2026";
        public string IkametgahAdresi { get; set; } = string.Empty;

        public string SaglikYardimStatüsü { get; set; } = string.Empty;

        public string Uyruk { get; set; } = "TC";
        [Required]
        public string OgrenciNo { get; set; } = string.Empty;
        public int OncekiStajGünSayisi { get; set; } = 0 ;




    }
}
