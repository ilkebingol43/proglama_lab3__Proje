using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab3_proje.Data;
using Lab3_proje.Models.Entities;
using Lab3_proje.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Lab3_proje.Services.Validation;

namespace Lab3_proje.Controllers
{
    // Sadece öğrenci rolü olanlar girebilir
    [Authorize(Roles = "ROLE_STUDENT")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        // constructor metot
        public StudentController(AppDbContext context)
        {
            _context = context;
        }



        //ogrenci ana sayfası
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.OgrenciDetay)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var sonBasvuru = await _context.StajBasvurulari
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.YuklemeTarihi)
                .FirstOrDefaultAsync();

            ViewBag.Ogrenci = user;
            return View(sonBasvuru);
        }



        // Evrak yükleme ekranını açar (Tamamlandı kontrolü ile)
        [HttpGet]
        public async Task<IActionResult> EvrakYukle()
        {
            int userId = GetCurrentUserId();

            var sonBasvuru = await _context.StajBasvurulari
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.YuklemeTarihi)
                .FirstOrDefaultAsync();

            if (sonBasvuru != null && sonBasvuru.Statu.Equals("tamamlandi", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Hata"] = "Staj başvurunuz 'Tamamlandı' statüsünde olduğu için evrak üzerinde değişiklik yapamazsınız.";
                return RedirectToAction("Index");
            }

            return View(new EvrakYukleViewModel());
        }

        // Evrak yükleme formunu kaydeder
        [HttpPost]
        public async Task<IActionResult> EvrakYukle(EvrakYukleViewModel model)
        {
            int userId = GetCurrentUserId();

            var sonBasvuru = await _context.StajBasvurulari
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.YuklemeTarihi)
                .FirstOrDefaultAsync();

            if (sonBasvuru != null && sonBasvuru.Statu.Equals("tamamlandi", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Hata"] = "Staj başvurunuz 'Tamamlandı' statüsünde olduğu için evrak üzerinde değişiklik yapamazsınız.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.BelgeDosyasi == null || model.BelgeDosyasi.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir dosya seçin.");
                return View(model);
            }

            // Dosya uzantı kontrolü
            var izinliUzantilar = new[] { ".doc", ".docx", ".pdf", ".jpeg", ".jpg", ".png" };
            var uzanti = Path.GetExtension(model.BelgeDosyasi.FileName).ToLowerInvariant();

            if (!izinliUzantilar.Contains(uzanti))
            {
                ModelState.AddModelError("", "Sadece Word, PDF, JPEG ve PNG dosyaları yükleyebilirsiniz.");
                return View(model);
            }

            // Bitiş tarihi kontrolü
            if (model.StajBitisTarihi < model.StajBaslangicTarihi)
            {
                ModelState.AddModelError("", "Staj bitiş tarihi başlangıç tarihinden önce olamaz.");
                return View(model);
            }

            // En az bir gün seçilmiş mi kontrolü
            if (model.SecilenGunler == null || model.SecilenGunler.Count == 0)
            {
                ModelState.AddModelError("", "Lütfen staj yapılacak en az bir gün seçiniz.");
                return View(model);
            }

            // 30 gün kuralı kontrolü
            KuralKontrolcu tarihKontrolcu = new StajTarihKontrolcu();
            if (!tarihKontrolcu.KontrolEt(model.StajBaslangicTarihi, out string hataTarih))
            {
                ModelState.AddModelError("", hataTarih);
                return View(model);
            }

            // Öğretim yılı format kontrolü
            KuralKontrolcu ogretimYiliKontrolcu = new OgretimYiliKontrolcu();
            if (!ogretimYiliKontrolcu.KontrolEt(model.OgretimYili, out string hataYil))
            {
                ModelState.AddModelError("", hataYil);
                return View(model);
            }

            // Seçilen günlere göre resmi tatiller hariç staj iş günü sayısını hesaplama
            int hesaplananGun = 0;
            int dusulenTatilSayisi = 0;
            var kultur = new System.Globalization.CultureInfo("tr-TR");

            for (var tarih = model.StajBaslangicTarihi.Date; tarih <= model.StajBitisTarihi.Date; tarih = tarih.AddDays(1))
            {
                string gunAdi = tarih.ToString("dddd", kultur);
                if (model.SecilenGunler.Contains(gunAdi))
                {
                    if (ResmiTatilServisi.TatilMi(tarih, out _))
                    {
                        dusulenTatilSayisi++;
                    }
                    else
                    {
                        hesaplananGun++;
                    }
                }
            }

            // Dosyayı wwwroot/uploads klasörüne kaydetme
            var yuklemeKlasoru = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(yuklemeKlasoru))
            {
                Directory.CreateDirectory(yuklemeKlasoru);
            }

            var dosyaAdi = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.BelgeDosyasi.FileName);
            var tamYol = Path.Combine(yuklemeKlasoru, dosyaAdi);

            using (var stream = new FileStream(tamYol, FileMode.Create))
            {
                await model.BelgeDosyasi.CopyToAsync(stream);
            }

            var basvuru = new StajBasvuru
            {
                UserId = userId,
                DosyaAdi = model.BelgeDosyasi.FileName,
                DosyaYolu = "/uploads/" + dosyaAdi,
                DosyaUzantisi = uzanti,
                YuklemeTarihi = DateTime.Now,
                Statu = "yüklendi",
                StajBaslangicTarihi = model.StajBaslangicTarihi,
                StajBitisTarihi = model.StajBitisTarihi,
                SecilenHaftalikGunler = string.Join(",", model.SecilenGunler),
                HesaplanmısStajGunSayısı = hesaplananGun,
                raporMesajı = $"Dönem: {model.OgretimYili} {model.Yariyil}"
            };

            _context.StajBasvurulari.Add(basvuru);
            await _context.SaveChangesAsync();

            string tatilBilgisi = dusulenTatilSayisi > 0 ? $" ({dusulenTatilSayisi} gün resmi tatil hesaptan düşüldü)" : "";
            TempData["Mesaj"] = $"Ek-16 belgeniz başarıyla yüklendi. Hesaplanan staj süresi: {hesaplananGun} iş günü{tatilBilgisi}.";
            return RedirectToAction("Index");
        }
        // Giriş yapan öğrencinin id bilgisini alır
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}