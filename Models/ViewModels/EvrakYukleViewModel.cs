using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Lab3_proje.Models.ViewModels
{
    public class EvrakYukleViewModel
    {
        [Required(ErrorMessage = "Lütfen Ek-16 belgenizi seçiniz.")]
        [Display(Name = "Ek-16 Belgesi")]
        public IFormFile? BelgeDosyasi { get; set; }

        [Required(ErrorMessage = "Öğretim yılı zorunludur.")]
        [Display(Name = "Öğretim Yılı")]
        public string OgretimYili { get; set; } = $"{DateTime.Today.Year}-{DateTime.Today.Year + 1}";

        [Required(ErrorMessage = "Yarıyıl seçimi zorunludur.")]
        [Display(Name = "Yarıyıl")]
        public string Yariyil { get; set; } = "Bahar";

        [Required(ErrorMessage = "Staj başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Staj Başlangıç Tarihi")]
        public DateTime StajBaslangicTarihi { get; set; } = DateTime.Today.AddDays(31);

        [Required(ErrorMessage = "Staj bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Staj Bitiş Tarihi")]
        public DateTime StajBitisTarihi { get; set; } = DateTime.Today.AddDays(60);

        [Display(Name = "Staj Yapılacak Günler")]
        public List<string> SecilenGunler { get; set; } = new List<string>();

        [Display(Name = "Staj Evrağı Teslim Google Form URL'si")]
        public string? GoogleFormUrl { get; set; }

        [Required(ErrorMessage = "Mühendis sayısı girmek zorunludur.")]
        [Range(1, 1000, ErrorMessage = "Mühendis sayısı en az 1 olmalıdır.")]
        [Display(Name = "İş Yerinde Çalışan Mühendis Sayısı")]
        public int MuhendisSayisi { get; set; } = 1;

        [Display(Name = "Mühendisin Mezun Olduğu Lisans Bölümü")]
        public string? MuhendisMezuniyetBolumu { get; set; }
    }
}