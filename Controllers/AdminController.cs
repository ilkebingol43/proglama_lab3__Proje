using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab3_proje.Data;
using Lab3_proje.Models.Entities;
using Lab3_proje.Models.ViewModels;

namespace Lab3_proje.Controllers
{
    // Sadece admin rolü olanlar girebilir
    [Authorize(Roles = "ROLE_ADMIN")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        // constructor metot
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm staj başvurularını listeler ve filtreler
        public async Task<IActionResult> Index(string? arama, string? statuFiltre)
        {
            var sorgu = _context.StajBasvurulari.Include(b => b.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(arama))
            {
                sorgu = sorgu.Where(b => b.User!.Name.Contains(arama) || b.DosyaAdi.Contains(arama));
            }

            if (!string.IsNullOrWhiteSpace(statuFiltre))
            {
                sorgu = sorgu.Where(b => b.Statu == statuFiltre);
            }

            var basvurular = await sorgu.OrderByDescending(b => b.YuklemeTarihi).ToListAsync();
            return View(basvurular);
        }

        // Tamamlanan ve onaylanan belgeleri ayrı sayfada listeler
        public async Task<IActionResult> Tamamlananlar()
        {
            var liste = await _context.StajBasvurulari
                .Include(b => b.User)
                .Where(b => b.Statu == "Tamamlandi")
                .ToListAsync();

            return View(liste);
        }

        // Test veri seti yükleme ekranını açar
        [HttpGet]
        public IActionResult VeriSetiYukle()
        {
            return View(new VeriSetiYukleViewModel());
        }

        // CSV formatındaki öğrenci listesini okur ve veritabanına ekler
        [HttpPost]
        public async Task<IActionResult> VeriSetiYukle(VeriSetiYukleViewModel model)
        {
            if (!ModelState.IsValid || model.VeriDosyasi == null)
            {
                ModelState.AddModelError("", "Lütfen bir CSV dosyası seçin.");
                return View(model);
            }

            try
            {
                using (var reader = new StreamReader(model.VeriDosyasi.OpenReadStream()))
                {
                    // Başlık satırını geç
                    var baslik = await reader.ReadLineAsync();

                    while (!reader.EndOfStream)
                    {
                        var satir = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(satir)) continue;

                        var sutunlar = satir.Split(',');
                        if (sutunlar.Length >= 7)
                        {
                            var adSoyad = sutunlar[0].Trim();
                            var tc = sutunlar[1].Trim();
                            var uyruk = sutunlar[2].Trim();
                            var ogrNo = sutunlar[3].Trim();
                            var bolum = sutunlar[4].Trim();
                            var tel = sutunlar[5].Trim();
                            var email = sutunlar[6].Trim();

                            // Eğer bu email ile öğrenci yoksa yeni kayıt aç
                            if (!_context.Users.Any(u => u.Email == email))
                            {
                                var yeniOgrenci = new User
                                {
                                    Name = adSoyad,
                                    UserName = ogrNo,
                                    Email = email,
                                    Role = "ROLE_STUDENT"
                                };
                                yeniOgrenci.SifreyiHashle("Ogrenci123!");

                                var detay = new OgrenciDetay
                                {
                                    TCKimlik = tc,
                                    Uyruk = uyruk,
                                    OgrenciNo = ogrNo,
                                    OkuduguBolum = bolum,
                                    TelefonNumarası = tel
                                };

                                yeniOgrenci.OgrenciDetay = detay;
                                _context.Users.Add(yeniOgrenci);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Basari"] = "Öğrenci veri seti başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Dosya okunurken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }
    }
}