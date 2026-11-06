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

        // ANA GİRİŞ EKRANI
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

        // ÖĞRENCİ REGİSTER
        [HttpGet]
        public IActionResult OgrenciRegister()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public IActionResult OgrenciRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kurallar sınıfı için geçici User nesnesi
            var tempUser = new User
            {
                Name = model.Name,
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password
            };

            // İsim Kuralı (Sadece harf ve boşluk, 2-50 karakter)
            if (!_kurallar.IsimKontrol(tempUser))
            {
                ViewBag.Hata = "İsim sadece harflerden oluşmalı ve 2-50 karakter arasında olmalıdır!";
                return View(model);
            }

            // Şifre Kuralı (8-12 karakter, en az 1 büyük harf ve 1 özel karakter)
            if (!_kurallar.PasswordKontrol(tempUser))
            {
                ViewBag.Hata = "Şifre 8-12 karakter olmalı, en az 1 büyük harf ve 1 özel karakter içermelidir!";
                return View(model);
            }

            // E-posta formatı kuralı (Regex)
            if (!_kurallar.EpostaKontrol(tempUser))
            {
                ViewBag.Hata = "Lütfen geçerli bir e-posta formatı giriniz (örn: ad@alanadi.uzanti)!";
                return View(model);
            }

            // Yeni kullanıcı nesnesi oluşturma
            var newUser = new User
            {
                Name = model.Name,
                UserName = model.UserName,
                Email = model.Email,
                Role = "ROLE_STUDENT"
            };

            newUser.BoslukTemizle(newUser.Name);
            newUser.SifreyiHashle(model.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return RedirectToAction("OgrenciLogin");
        }

        // ADMİN REGİSTER
        [HttpGet]
        public IActionResult AdminRegister()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public IActionResult AdminRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // kullanıcı adı kontrolü
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ViewBag.Hata = $"'{model.UserName}' kullanıcı adı zaten alınmış! Başka bir kullanıcı adı seçin.";
                return View(model);
            }

            // eposta kontrolü
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Hata = "Bu e-posta adresiyle zaten kayıtlı bir hesap var!";
                return View(model);
            }

            var tempUser = new User
            {
                Name = model.Name,
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password
            };

            // kurallar kontrolü
            if (!_kurallar.IsimKontrol(tempUser))
            {
                ViewBag.Hata = "İsim sadece harflerden oluşmalı ve 2-50 karakter arasında olmalıdır!";
                return View(model);
            }

            if (!_kurallar.PasswordKontrol(tempUser))
            {
                ViewBag.Hata = "Şifre 8-12 karakter olmalı, en az 1 büyük harf ve 1 özel karakter içermelidir!";
                return View(model);
            }

            if (!_kurallar.EpostaKontrol(tempUser))
            {
                ViewBag.Hata = "Lütfen geçerli bir e-posta formatı giriniz (örn: ad@alanadi.uzanti)!";
                return View(model);
            }

            var newAdmin = new User
            {
                Name = model.Name,
                UserName = model.UserName,
                Email = model.Email,
                Role = "ROLE_ADMIN"
            };

            newAdmin.BoslukTemizle(newAdmin.Name);
            newAdmin.SifreyiHashle(model.Password);

            _context.Users.Add(newAdmin);
            _context.SaveChanges();

            return RedirectToAction("AdminLogin");
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