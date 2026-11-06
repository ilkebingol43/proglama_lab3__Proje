using Lab3_proje.Data;
using Lab3_proje.Models.Entities;
using Lab3_proje.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        // =============================================================
        // KAYIT İŞLEMLERİ (Register)
        // =============================================================

        // 1. Boş Kayıt Formunu Gösterir
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 2. Kayıt Formunu Gönderir ve Veritabanına Yazar
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

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // =============================================================
        // ANA GİRİŞ SEÇİM EKRANI (İki Butonlu Sayfa)
        // =============================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =============================================================
        // ÖĞRENCİ GİRİŞ İŞLEMLERİ
        // =============================================================
        [HttpGet]
        public IActionResult OgrenciLogin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> OgrenciLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcı adı veya E-posta ile ara ve SADECE ROLE_STUDENT olanı bul
            var user = _context.Users.FirstOrDefault(u =>
                (u.UserName == model.UsernameOrEmail || u.Email == model.UsernameOrEmail)
                && u.Role == "ROLE_STUDENT");

            if (user == null)
            {
                ViewBag.Hata = "Öğrenci kullanıcı adı / e-posta veya şifre hatalı!";
                return View(model);
            }

            // Hesap kilitli mi kontrolü (15 dakika)
            if (user.KilitlendiMi)
            {
                if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
                {
                    var kalanDakika = (int)Math.Ceiling((user.LockoutEndTime.Value - DateTime.Now).TotalMinutes);
                    ViewBag.Hata = $"Hesabınız 5 hatalı giriş nedeniyle kilitlenmiştir! Lütfen {kalanDakika} dakika sonra tekrar deneyiniz.";
                    return View(model);
                }
                else
                {
                    user.GirisSayisiSifirlama();
                    _context.SaveChanges();
                }
            }

            // Şifre doğrulama (BCrypt)
            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

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
                return View(model);
            }

            // Giriş başarılı: Hataları sıfırla
            user.GirisSayisiSifirlama();
            _context.SaveChanges();

            // Cookie ile oturum başlat
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("Index", "Student");
        }

        // =============================================================
        // ADMIN GİRİŞ İŞLEMLERİ
        // =============================================================
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcı adı veya E-posta ile ara ve SADECE ROLE_ADMIN olanı bul
            var user = _context.Users.FirstOrDefault(u =>
                (u.UserName == model.UsernameOrEmail || u.Email == model.UsernameOrEmail)
                && u.Role == "ROLE_ADMIN");

            if (user == null)
            {
                ViewBag.Hata = "Admin bilgileri veya şifre hatalı!";
                return View(model);
            }

            // Hesap kilitli mi kontrolü (15 dakika)
            if (user.KilitlendiMi)
            {
                if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
                {
                    var kalanDakika = (int)Math.Ceiling((user.LockoutEndTime.Value - DateTime.Now).TotalMinutes);
                    ViewBag.Hata = $"Hesabınız 5 hatalı giriş nedeniyle kilitlenmiştir! Lütfen {kalanDakika} dakika sonra tekrar deneyiniz.";
                    return View(model);
                }
                else
                {
                    user.GirisSayisiSifirlama();
                    _context.SaveChanges();
                }
            }

            // Şifre doğrulama (BCrypt)
            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

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
                return View(model);
            }

            // Giriş başarılı: Hataları sıfırla
            user.GirisSayisiSifirlama();
            _context.SaveChanges();

            // Cookie ile oturum başlat
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("Index", "Admin");
        }

        // =============================================================
        // ÇIKIŞ YAP (Logout) & YETKİSİZ ERİŞİM (AccessDenied)
        // =============================================================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}