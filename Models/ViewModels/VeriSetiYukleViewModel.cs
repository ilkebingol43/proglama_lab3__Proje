using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Lab3_proje.Models.ViewModels
{
    public class VeriSetiYukleViewModel
    {
        [Required(ErrorMessage = "Lütfen bir CSV dosyası seçin.")]
        [Display(Name = "CSV Veri Dosyası")]
        public IFormFile? VeriDosyasi { get; set; }
    }
}