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

        // --- 1. ÖĞRENCİ BİLGİLERİ (Ek-16 Formu 1. Bölüm) ---
        [Required(ErrorMessage = "Öğrenci adı soyadı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string OgrenciAdiSoyadi { get; set; } = string.Empty;

        [Required(ErrorMessage = "T.C. Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. Kimlik No 11 haneli olmalıdır.")]
        [Display(Name = "T.C. Kimlik No")]
        public string TcKimlikNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci numarası zorunludur.")]
        [Display(Name = "Öğrenci No")]
        public string OgrenciNo { get; set; } = string.Empty;

        [Display(Name = "Uyruk")]
        public string Uyruk { get; set; } = "T.C.";

        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Okuduğu Bölüm")]
        public string Bolum { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Eposta { get; set; } = string.Empty;

        [Display(Name = "Telefon No")]
        public string TelefonNo { get; set; } = string.Empty;

        [Display(Name = "İkametgâh Adresi")]
        public string IkametgahAdresi { get; set; } = string.Empty;

        [Display(Name = "Önceki Staj Gün Sayısı")]
        public int OncekiStajGunSayisi { get; set; } = 0;

        [Display(Name = "Sağlık Yardımı Statüsü")]
        public string SaglikYardimStatusu { get; set; } = "Aileden Sağlık Yardımı Alıyor";

        // --- 2. KURUM / İŞ YERİ BİLGİLERİ ---
        [Required(ErrorMessage = "Staj yapılacak kurum / firma adı zorunludur.")]
        [Display(Name = "Staj Yapılacak Kurum / Şirket Adı")]
        public string KurumAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mühendis sayısı girmek zorunludur.")]
        [Range(1, 1000, ErrorMessage = "Mühendis sayısı en az 1 olmalıdır.")]
        [Display(Name = "İş Yerinde Çalışan Mühendis Sayısı")]
        public int MuhendisSayisi { get; set; } = 1;

        [Display(Name = "Mühendisin Mezun Olduğu Lisans Bölümü")]
        public string? MuhendisMezuniyetBolumu { get; set; }

        // --- 3. STAJ TARİH VE DÖNEM BİLGİLERİ ---
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
    }
}