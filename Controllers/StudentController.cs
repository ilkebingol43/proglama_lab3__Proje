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

            // Word (.docx) belgesi ise metin taraması ve öğrenci bilgisi karşılaştırması
            string belgeUyarisi = "";
            if (uzanti == ".docx")
            {
                try
                {
                    using (var archive = new System.IO.Compression.ZipArchive(model.BelgeDosyasi.OpenReadStream()))
                    {
                        var entry = archive.GetEntry("word/document.xml");
                        if (entry != null)
                        {
                            using (var reader = new StreamReader(entry.Open()))
                            {
                                string xmlIcerik = await reader.ReadToEndAsync();
                                string metin = System.Text.RegularExpressions.Regex.Replace(xmlIcerik, "<.*?>", " ");

                                var ogrenci = await _context.Users
                                    .Include(u => u.OgrenciDetay)
                                    .FirstOrDefaultAsync(u => u.Id == userId);

                                if (ogrenci != null)
                                {
                                    bool isimVar = !string.IsNullOrWhiteSpace(ogrenci.Name) &&
                                                   metin.IndexOf(ogrenci.Name, StringComparison.OrdinalIgnoreCase) >= 0;

                                    bool noVar = !string.IsNullOrWhiteSpace(ogrenci.OgrenciDetay?.OgrenciNo) &&
                                                 metin.IndexOf(ogrenci.OgrenciDetay.OgrenciNo, StringComparison.OrdinalIgnoreCase) >= 0;

                                    if (!isimVar && !noVar)
                                    {
                                        belgeUyarisi = " | [UYARI: Belgedeki öğrenci bilgileri sistemdeki hesabınızla uyuşmuyor!]";
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Belge okuma hatasında akışı kesme
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
                Statu = "Yuklendi",
                StajBaslangicTarihi = model.StajBaslangicTarihi,
                StajBitisTarihi = model.StajBitisTarihi,
                SecilenHaftalikGunler = string.Join(",", model.SecilenGunler),
                HesaplanmısStajGunSayısı = hesaplananGun,
                raporMesajı = $"Dönem: {model.OgretimYili} {model.Yariyil}{belgeUyarisi}"
            };

            _context.StajBasvurulari.Add(basvuru);
            await _context.SaveChangesAsync();

            string tatilBilgisi = dusulenTatilSayisi > 0 ? $" ({dusulenTatilSayisi} gün resmi tatil hesaptan düşüldü)" : "";

            if (!string.IsNullOrEmpty(belgeUyarisi))
            {
                TempData["Hata"] = "Ek-16 belgeniz yüklendi fakat belgedeki isim/öğrenci numarası hesabınızla uyuşmuyor!";
            }
            else
            {
                TempData["Mesaj"] = $"Ek-16 belgeniz başarıyla yüklendi. Hesaplanan staj süresi: {hesaplananGun} iş günü{tatilBilgisi}.";
            }

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