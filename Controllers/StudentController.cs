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

        // Öğrencinin ana sayfası, yüklediği son başvuruyu gösterir
        public IActionResult Index()
        {
            int userId = GetCurrentUserId();
            var sonBasvuru = _context.StajBasvurulari
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.YuklemeTarihi)
                .FirstOrDefault();

            return View(sonBasvuru);
        }

        // Evrak yükleme ekranını açar
        [HttpGet]
        public IActionResult EvrakYukle()
        {
            return View(new EvrakYukleViewModel());
        }

        // Evrak yükleme formunu kaydeder
        [HttpPost]
        public async Task<IActionResult> EvrakYukle(EvrakYukleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.BelgeDosyasi == null || model.BelgeDosyasi.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir dosya seçin.");
                return View(model);
            }

            // İzin verilen dosya uzantıları
            var izinliUzantilar = new[] { ".doc", ".docx", ".pdf", ".jpeg", ".jpg", ".png" };
            var uzanti = Path.GetExtension(model.BelgeDosyasi.FileName).ToLowerInvariant();

            if (!izinliUzantilar.Contains(uzanti))
            {
                ModelState.AddModelError("", "Sadece Word, PDF, JPEG ve PNG dosyaları yükleyebilirsiniz.");
                return View(model);
            }

            // Staj başlangıcından en az 30 gün önce yüklenmeli
            if ((model.StajBaslangicTarihi.Date - DateTime.Today).TotalDays < 30)
            {
                DateTime sonTarih = model.StajBaslangicTarihi.AddDays(-30);
                ModelState.AddModelError("", $"Evrak staj başlangıcından en az 30 gün önce yüklenmelidir. En geç yükleme tarihi: {sonTarih:dd.MM.yyyy}");
                return View(model);
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

            int userId = GetCurrentUserId();

            // Veritabanına yeni başvuru ekle
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
                SecilenHaftalikGunler = string.Join(",", model.SecilenGunler)
            };

            _context.StajBasvurulari.Add(basvuru);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Belgeniz yüklendi, inceleme aşamasına alındı.";
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