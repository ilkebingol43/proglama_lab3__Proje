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
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Kurallar _kurallar;

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

        // 2. KAYDETMEK İÇİN ÇALIŞIR: Kullanıcı formu doldurup "Kayıt Ol" butonuna bastığında burası çalışır
        [HttpPost]
        public IActionResult Register(User model)
        {
            // KURAL 1: Kullanıcı adı daha önce alınmış mı?
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ViewBag.Hata = $"'{model.UserName}' kullanıcı adı zaten alınmış! Başka bir kullanıcı adı seçin.";
                return View(model);
            }

            // KURAL 2: E-Posta daha önce alınmış mı?
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Hata = "Bu e-posta adresiyle zaten kayıtlı bir hesap var!";
                return View(model);
            }

            // KURAL 3: İsim Kuralı (Sadece harf ve boşluk, 2-50 karakter)
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

            // Şifreyi BCrypt ile hashliyoruz
            model.SifreyiHashle(model.Password);

            // Yeni kaydolan herkes Öğrenci rolünü alır
            model.Role = "ROLE_STUDENT";

            // Veritabanına ekle ve kaydet
            _context.Users.Add(model);
            _context.SaveChanges();

            // Kayıt başarılı! Kullanıcıyı Giriş Yap (Login) sayfasına yönlendir
            return RedirectToAction("Login");
        }

        // GİRİŞ EKRANI
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // GİRİŞ KONTROLÜ (5 Hatalı Giriş Kilidi ve Cookie Başlatma)
        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdiVeyaEmail, string sifre)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == kullaniciAdiVeyaEmail || u.Email == kullaniciAdiVeyaEmail);

            if (user == null)
            {
                ViewBag.Hata = "Kullanıcı adı / e-posta veya şifre hatalı!";
                return View();
            }

            // HESAP KİLİTLİ Mİ? (15 dakika kontrolü)
            if (user.KilitlendiMi)
            {
                if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
                {
                    var kalanDakika = (int)Math.Ceiling((user.LockoutEndTime.Value - DateTime.Now).TotalMinutes);
                    ViewBag.Hata = $"Hesabınız 5 hatalı giriş nedeniyle kilitlenmiştir! Lütfen {kalanDakika} dakika sonra tekrar deneyiniz.";
                    return View();
                }
                else
                {
                    user.GirisSayisiSifirlama();
                    _context.SaveChanges();
                }
            }

            // ŞİFRE DOĞRULAMA (BCrypt)
            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(sifre, user.PasswordHash);

            if (!sifreDogruMu)
            {
                user.GirisLoglama();
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

            // GİRİŞ BAŞARILI
            user.GirisSayisiSifirlama();
            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (user.Role == "ROLE_ADMIN")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Student");
        }

        // ÇIKIŞ YAP (Logout)
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // 403 ERİŞİM ENGELLENDİ
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}