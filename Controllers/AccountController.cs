using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Lab3_proje.Data;
using Lab3_proje.Models.Entities;
using Lab3_proje.Models.ViewModels;

namespace Lab3_proje.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Kurallar _kurallar;

        public AccountController(AppDbContext context)
        {
            _context = context;
            _kurallar = new Kurallar();
        }

       
        
        [HttpGet]
        public IActionResult Login()
        {
            // Kullanıcı zaten giriş yapmışsa rolüne göre yönlendir
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("ROLE_ADMIN"))
                    return RedirectToAction("Index", "Admin");
                if (User.IsInRole("ROLE_STUDENT"))
                    return RedirectToAction("Index", "Student");
            }
            return View();
        }

      
        [HttpGet]
        public IActionResult OgrenciLogin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> OgrenciLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            
            var user = _context.Users.FirstOrDefault(u =>
                (u.UserName == model.UsernameOrEmail || u.Email == model.UsernameOrEmail)
                && u.Role == "ROLE_STUDENT");

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı/e-posta veya şifre hatalı!");
                return View(model);
            }


            if (user.KilitlendiMi)
            {
                if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
                {
                    var kalanDakika = (int)Math.Ceiling((user.LockoutEndTime.Value - DateTime.Now).TotalMinutes);
                    ModelState.AddModelError("", $"Hesabınız 5 hatalı giriş nedeniyle kilitlenmiştir! Lütfen {kalanDakika} dakika sonra tekrar deneyiniz.");
                    return View(model);
                }
                else
                {
                    user.GirisSayisiSifirlama();
                    _context.SaveChanges();
                }
            }

            // BCrypt Şifre Doğrulama
            bool sifreDogru = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!sifreDogru)
            {
                user.GirisLoglama();
                _context.SaveChanges();

                if (user.KilitlendiMi)
                {
                    ModelState.AddModelError("", "5 kez hatalı şifre girdiniz! Hesabınız 15 dakika kilitlendi.");
                }
                else
                {
                    int kalan = 5 - user.FailedLoginAttempts;
                    ModelState.AddModelError("", $"Hatalı şifre! Kalan deneme hakkınız: {kalan}");
                }
                return View(model);
            }

            // Giriş Başarılı: Hataları sıfırla
            user.GirisSayisiSifirlama();
            _context.SaveChanges();

            // Cookie ile Oturum Açma
            await OturumAcAsync(user);

            return RedirectToAction("Index", "Student");
        }


            //Admin giriş ekranı
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Sadece ROLE_ADMIN rolündekileri ara
            var user = _context.Users.FirstOrDefault(u =>
                (u.UserName == model.UsernameOrEmail || u.Email == model.UsernameOrEmail)
                && u.Role == "ROLE_ADMIN");

            if (user == null)
            {
                ModelState.AddModelError("", "Admin bilgileri veya şifre hatalı!");
                return View(model);
            }

            // Kilit kontrolü
            if (user.KilitlendiMi && user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
            {
                var kalanDakika = (int)Math.Ceiling((user.LockoutEndTime.Value - DateTime.Now).TotalMinutes);
                ModelState.AddModelError("", $"Admin hesabı kilitli! Kalan süre: {kalanDakika} dakika.");
                return View(model);
            }

            // Şifre kontrolü
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                user.GirisLoglama();
                _context.SaveChanges();
                ModelState.AddModelError("", "Admin şifresi hatalı!");
                return View(model);
            }

            user.GirisSayisiSifirlama();
            _context.SaveChanges();

            await OturumAcAsync(user);

            return RedirectToAction("Index", "Admin");
        }

        //Kayıtİşlemleri
        // Kayıt Ol Ekranı
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ViewBag.Hata = $"'{model.UserName}' kullanıcı adı zaten alınmış!";
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Hata = "Bu e-posta adresiyle kayıtlı bir hesap var!";
                return View(model);
            }

            if (!_kurallar.IsimKontrol(model))
            {
                ViewBag.Hata = "İsim sadece harflerden oluşmalı ve 2-50 karakter arasında olmalıdır!";
                return View(model);
            }

            if (!_kurallar.PasswordKontrol(model))
            {
                ViewBag.Hata = "Şifre 8-12 karakter olmalı, en az 1 büyük harf ve 1 özel karakter içermelidir!";
                return View(model);
            }

            if (!_kurallar.EpostaKontrol(model))
            {
                ViewBag.Hata = "Geçerli bir e-posta formatı giriniz!";
                return View(model);
            }

            model.SifreyiHashle(model.Password);
            model.Role = "ROLE_STUDENT";

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("OgrenciLogin");
        }

      
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task OturumAcAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}