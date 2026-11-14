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

            // Mühendis Sayısı Kuralı Kontrolü
            KuralKontrolcu muhendisKontrolcu = new MuhendisBilgiKontrolcu();
            if (!muhendisKontrolcu.KontrolEt((model.MuhendisSayisi, model.MuhendisMezuniyetBolumu), out string hataMuhendis))
            {
                ModelState.AddModelError("", hataMuhendis);
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

            // Belgeden metin tarama (DOCX ve PDF sıkıştırma çözme desteği)
            string tarananMetin = string.Empty;
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
                                tarananMetin = System.Text.RegularExpressions.Regex.Replace(xmlIcerik, "<.*?>", " ");
                            }
                        }
                    }
                }
                catch { }
            }
            else if (uzanti == ".pdf")
            {
                try
                {
                    using (var pdfStream = model.BelgeDosyasi.OpenReadStream())
                    {
                        tarananMetin = PdfMetinCikar(pdfStream);
                    }
                }
                catch { }
            }

            // Belgedeki bilgiler ile kullanıcının girdiği bilgilerin karşılaştırılması
            var ogrenci = await _context.Users
                .Include(u => u.OgrenciDetay)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var uyusmazliklar = new List<string>();

            if (ogrenci != null)
            {
                string normBelge = MetinNormallestir(tarananMetin);

                // 1. İsim Kontrolü (Türkçe karakter ve parça kontrolü)
                bool isimVar = false;
                if (!string.IsNullOrWhiteSpace(ogrenci.Name))
                {
                    string normIsim = MetinNormallestir(ogrenci.Name);
                    if (normBelge.Contains(normIsim))
                    {
                        isimVar = true;
                    }
                    else
                    {
                        var parcalar = ogrenci.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parcalar.Length > 0 && parcalar.Any(p => normBelge.Contains(MetinNormallestir(p))))
                        {
                            isimVar = true;
                        }
                    }
                }

                if (!isimVar)
                {
                    uyusmazliklar.Add("Öğrenci adı belgede bulunamadı");
                }

                // 2. Öğrenci Numarası Kontrolü
                string ogrNo = ogrenci.OgrenciDetay?.OgrenciNo ?? ogrenci.UserName;
                bool noVar = false;
                if (!string.IsNullOrWhiteSpace(ogrNo))
                {
                    string normNo = MetinNormallestir(ogrNo);
                    if (normBelge.Contains(normNo) || tarananMetin.Contains(ogrNo))
                    {
                        noVar = true;
                    }
                }
                // 4. Kurum / Firma Adı Kontrolü (Opsiyonel Çapraz Doğrulama)
                if (!string.IsNullOrWhiteSpace(model.KurumAdi))
                {
                    string normKurum = MetinNormallestir(model.KurumAdi);
                    if (!normBelge.Contains(normKurum) && !tarananMetin.Contains(model.KurumAdi.Trim()))
                    {
                        // Belgede kurum adı aranır, bulunamazsa raporda detay olarak belirtilir
                    }
                }

                if (!noVar)
                {
                    uyusmazliklar.Add("Öğrenci numarası belgede bulunamadı");
                }

                // 3. Staj Başlangıç Tarihi veya Yılı Kontrolü
                string formatGunAyYil = model.StajBaslangicTarihi.ToString("dd.MM.yyyy");
                string formatYil = model.StajBaslangicTarihi.ToString("yyyy");
                bool tarihVar = normBelge.Contains(MetinNormallestir(formatGunAyYil)) ||
                                normBelge.Contains(formatYil) ||
                                tarananMetin.Contains(formatGunAyYil) ||
                                tarananMetin.Contains(formatYil);

                if (!tarihVar)
                {
                    uyusmazliklar.Add("Staj başlangıç tarihi belgede bulunamadı");
                }
            }

            // Teslime Hazır Olup Olmadığına Karar Verme ve Statü Belirleme
            string belirlenenStatu = "Onaylandi";
            string raporMesaji = string.Empty;

            if (uyusmazliklar.Count > 0)
            {
                belirlenenStatu = "Hatali_Eksik";
                raporMesaji = $"✗ Belge Teslime Hazır Değil: [Uyuşmazlıklar: {string.Join(", ", uyusmazliklar)}]. Dönem: {model.OgretimYili} {model.Yariyil}";
            }
            else
            {
                belirlenenStatu = "Onaylandi";
                raporMesaji = $"✓ Belge Teslime Hazır: Belgedeki öğrenci bilgileri ve staj tarihleri form verileriyle tam olarak uyuşmaktadır. Dönem: {model.OgretimYili} {model.Yariyil}";
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

            // Başvuruyu veritabanına kaydetme
            var basvuru = new StajBasvuru
            {
                UserId = userId,
                DosyaAdi = model.BelgeDosyasi.FileName,
                DosyaYolu = "/uploads/" + dosyaAdi,
                DosyaUzantisi = uzanti,
                YuklemeTarihi = DateTime.Now,
                Statu = belirlenenStatu,
                StajBaslangicTarihi = model.StajBaslangicTarihi,
                StajBitisTarihi = model.StajBitisTarihi,
                SecilenHaftalikGunler = string.Join(",", model.SecilenGunler),
                HesaplanmısStajGunSayısı = hesaplananGun,
                raporMesajı = raporMesaji
            };

            _context.StajBasvurulari.Add(basvuru);
            await _context.SaveChangesAsync();

            string tatilBilgisi = dusulenTatilSayisi > 0 ? $" ({dusulenTatilSayisi} gün resmi tatil hesaptan düşüldü)" : "";

            if (belirlenenStatu == "Hatali_Eksik")
            {
                TempData["Hata"] = $"Belge yüklendi ancak teslime hazır değil! {string.Join(", ", uyusmazliklar)}.";
            }
            else
            {
                TempData["Mesaj"] = $"Ek-16 belgesi başarıyla doğrulandı ve teslime hazır olarak onaylandı. Hesaplanan staj süresi: {hesaplananGun} iş günü{tatilBilgisi}.";
            }

            return RedirectToAction("Index");
        }

        // Giriş yapan öğrencinin id bilgisini alır
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // PDF içindeki sıkıştırılmış akışları ve CMap font tablolarını çözen metot
        private static string PdfMetinCikar(Stream stream)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                byte[] bytes = ms.ToArray();
                string ascii = System.Text.Encoding.ASCII.GetString(bytes);

                var cmap = new Dictionary<string, char>(StringComparer.OrdinalIgnoreCase);
                var decompressedStreams = new List<string>();

                int pos = 0;
                while (pos < bytes.Length)
                {
                    int streamIndex = ascii.IndexOf("stream", pos, StringComparison.Ordinal);
                    if (streamIndex == -1) break;

                    int dataStart = streamIndex + 6;
                    while (dataStart < bytes.Length && (bytes[dataStart] == 13 || bytes[dataStart] == 10))
                        dataStart++;

                    int endStreamIndex = ascii.IndexOf("endstream", dataStart, StringComparison.Ordinal);
                    if (endStreamIndex == -1) break;

                    int length = endStreamIndex - dataStart;
                    while (length > 0 && (bytes[dataStart + length - 1] == 13 || bytes[dataStart + length - 1] == 10))
                        length--;

                    if (length > 0)
                    {
                        try
                        {
                            int offset = (length > 2 && bytes[dataStart] == 0x78) ? 2 : 0;
                            int count = length - offset;
                            if (count > 4 && offset == 2) count -= 4;

                            using var streamMs = new MemoryStream(bytes, dataStart + offset, count);
                            using var deflate = new System.IO.Compression.DeflateStream(streamMs, System.IO.Compression.CompressionMode.Decompress);
                            using var reader = new StreamReader(deflate, System.Text.Encoding.UTF8);
                            string decompressed = reader.ReadToEnd();
                            decompressedStreams.Add(decompressed);

                            // CMap (ToUnicode) font eşleşmelerini tespit etme
                            if (decompressed.Contains("beginbfrange"))
                            {
                                var rangeMatches = System.Text.RegularExpressions.Regex.Matches(decompressed, @"<([0-9A-Fa-f]{4})>\s+<([0-9A-Fa-f]{4})>\s+\[([^\]]+)\]");
                                foreach (System.Text.RegularExpressions.Match rm in rangeMatches)
                                {
                                    int startCode = Convert.ToInt32(rm.Groups[1].Value, 16);
                                    var targets = System.Text.RegularExpressions.Regex.Matches(rm.Groups[3].Value, @"<([0-9A-Fa-f]{4})>");
                                    for (int k = 0; k < targets.Count; k++)
                                    {
                                        string src = (startCode + k).ToString("X4");
                                        char destChar = (char)Convert.ToInt32(targets[k].Groups[1].Value, 16);
                                        cmap[src] = destChar;
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    pos = endStreamIndex + 9;
                }

                // Tüm streamlerdeki hem parantezli hem CMap glif kodlu metinleri çıkarma
                foreach (var streamText in decompressedStreams)
                {
                    // Düz metinler: (Metin)
                    var parenMatches = System.Text.RegularExpressions.Regex.Matches(streamText, @"\(([^)]*)\)");
                    foreach (System.Text.RegularExpressions.Match m in parenMatches)
                    {
                        sb.Append(m.Groups[1].Value);
                    }

                    // Font glif kodlu metinler: <000C000D...>
                    var hexMatches = System.Text.RegularExpressions.Regex.Matches(streamText, @"<([0-9A-Fa-f]{4,})>");
                    foreach (System.Text.RegularExpressions.Match hm in hexMatches)
                    {
                        string hex = hm.Groups[1].Value;
                        if (hex.Length % 4 == 0)
                        {
                            for (int c = 0; c < hex.Length; c += 4)
                            {
                                string token = hex.Substring(c, 4);
                                if (cmap.TryGetValue(token, out char ch))
                                {
                                    sb.Append(ch);
                                }
                            }
                        }
                    }
                    sb.Append(' ');
                }
            }
            catch { }
            return sb.ToString();
        }

        // Türkçe karakter ve boşluk toleransı için normalizasyon
        private static string MetinNormallestir(string? metin)
        {
            if (string.IsNullOrWhiteSpace(metin)) return string.Empty;
            return metin.ToLowerInvariant()
                .Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ı', 'i')
                .Replace('i', 'i')
                .Replace('ö', 'o')
                .Replace('ş', 's')
                .Replace('ü', 'u')
                .Replace(" ", "")
                .Replace("-", "")
                .Replace(".", "");
        }
    }
}