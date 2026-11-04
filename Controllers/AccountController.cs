using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Lab3_proje.Data;
using Lab3_proje.Models.Entities;


namespace Lab3_proje.Controllers
{
    public class AccountController : Controller // Kalıtım
    {
        private readonly AppDbContext _context ;
        private readonly Kurallar _kurallar ;
        
        public AccountController(AppDbContext context)
        {
            _kurallar = new Kurallar();
            _context = context;

        }

        // 1. EKRANI AÇAR: Kullanıcı /Account/Register adresine girdiğinde boş kayıt formunu gösterir
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 2. kaydetmek için çalışır : Kullanıcı formu doldurup "Kayıt Ol" butonuna bastığında burası çalışır
        [HttpPost]
        public IActionResult Register(User model)
        {
            //  Kullanıcı adı daha önce alınmış mı?
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ViewBag.Hata = $"'{model.UserName}' kullanıcı adı zaten alınmış! Başka bir kullanıcı adı seçin.";
                return View(model);
            }



            //  E-Posta daha önce alınmış mı?
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Hata = "Bu e-posta adresiyle zaten kayıtlı bir hesap var!";
                return View(model);
            }
            //  İsim Kuralı (Sadece harf ve boşluk, 2-50 karakter)
            if (!_kurallar.IsimKontrol(model))
            {
                ViewBag.Hata = "İsim sadece harflerden oluşmalı ve 2-50 karakter arasında olmalıdır!";
                return View(model);
            }


            // KURAL 4: Şifre Kuralı (8-12 karakter, en az 1 büyük harf ve 1 özel karakter)
            if (!_kurallar.PasswordKontrol(model))
            {
                ViewBag.Hata = "Şifre 8-12 karakter olmalı, en az 1 büyük harf ve 1 özel karakter içermelidir!";
                return View(model);
            }

            // KURAL 5: E-Posta Formatı Kuralı (Regex)
            if (!_kurallar.EpostaKontrol(model))
            {
                ViewBag.Hata = "Lütfen geçerli bir e-posta formatı giriniz (örn: ad@alanadi.uzanti)!";
                return View(model);
            }

            

            // Şifreyi açık metin bırakmıyoruz BCrypt ile hashliyoruz 
            model.SifreyiHashle(model.Password);

            // Yeni kaydolan herkes Öğrenci rolünü alır
            model.Role = "ROLE_STUDENT";

            // Veritabanına ekle ve kaydet
            _context.Users.Add(model);
            _context.SaveChanges();

            // Kayıt başarılı! Kullanıcıyı Giriş Yap (Login) sayfasına yönlendir
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // 2. GİRİŞ KONTROLÜ: Kullanıcı bilgileri yazıp "Giriş Yap" butonuna bastığında burası çalışır
        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdiVeyaEmail, string sifre)
        {
            // PDF Kuralı: Kullanıcı adı VEYA e-posta ile giriş yapılabilmeli
            var user = _context.Users.FirstOrDefault(u => u.UserName == kullaniciAdiVeyaEmail || u.Email == kullaniciAdiVeyaEmail);
            // KULLANICI YOKSA:
            if (user == null)
            {
                ViewBag.Hata = "Kullanıcı adı / e-posta veya şifre hatalı!";
                return View();
            }
            // GÜVENLİK KURALI: HESAP KİLİTLİ Mİ? (PDF: 5 Hatalı Giriş Kilidi)
            if (user.KilitlendiMi)
            {
                // Kilitlenme süresi dolmuş mu kontrol et (15 dakika doldu mu?)
                if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
                {
                    var kalanDakika = (int)Math.Ceiling((user.LockoutEndTime.Value - DateTime.Now).TotalMinutes);
                    ViewBag.Hata = $"Hesabınız 5 hatalı giriş nedeniyle kilitlenmiştir! Lütfen {kalanDakika} dakika sonra tekrar deneyiniz.";
                    return View();
                }
                else
                {
                    // 15 dakika dolmuş, kilidi kaldır ve sayacı sıfırla
                    user.GirisSayisiSifirlama();
                    _context.SaveChanges();
                }
            }
            // ŞİFRE KONTROLÜ: BCrypt ile girilen şifreyi hash ile eşleştiriyoruz
            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(sifre, user.PasswordHash);
            // ŞİFRE YANLIŞSA:
            if (!sifreDogruMu)
            {
                user.GirisLoglama(); // Hatalı giriş sayısını 1 artırır, 5 olunca hesabı kilitler
                _context.SaveChanges();
                if (user.KilitlendiMi)
                {
                    ViewBag.Hata = "5 kez hatalı şifre girdiniz! Hesabınız 15 dakika süreyle kilitlendi.";
                }
                else
                {
                    int kalanHak = 5 - user.FailedLoginAttempts;
                    ViewBag.Hata = $"Hatalı şifre! Kalan deneme hakkınız: {kalanHak}";
                }
                return View();
            }
            // --- ŞİFRE DOĞRU! GİRİŞ BAŞARILI ---
            // Başarılı girişte hatalı sayaçlarını sıfırlıyoruz
            user.GirisSayisiSifirlama();
            _context.SaveChanges();
            // PDF Kuralı: Giriş sonrası rol bilgisini oturuma (Cookie) yüklüyoruz
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role) // ROLE_ADMIN veya ROLE_STUDENT
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // Tarayıcı kapansa bile oturumu hatırla
            };
            // Dijital Kimlik Kartını (Cookie) tarayıcıya veriyoruz: Oturum Başladı!
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
           
            if (user.Role == "ROLE_ADMIN")
            {
                return RedirectToAction("Index", "Admin"); // Admin ise Admin Paneline
            }
            return RedirectToAction("Index", "Student"); // Öğrenci ise Öğrenci Paneline
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Cookie'yi imha et
            return RedirectToAction("Login");
        }
        // 403 ERİŞİM ENGELLENDİ (PDF Kuralı: Yetkisiz istekler 403 döner)
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(); // Mahsun'un 403 sayfasına gider
        }



    }
}
