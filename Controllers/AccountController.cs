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

       
        //kayıt islemleri
       

        // Kayıt ol Ekranı
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }





        // OGRENCİ REGİSTER
        // boş ogrenci kayıt formunu gösterir
        [HttpGet]
        public IActionResult OgrenciRegister()
        {
            return View(new RegisterViewModel());
        }

        // Öğrenci kayıt formunu doğrular ve veritabanına kaydeder
        [HttpPost]
        public IActionResult OgrenciRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcı adı daha önce alınmış mı
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ViewBag.Hata = $"'{model.UserName}' kullanıcı adı zaten alınmış! Başka bir kullanıcı adı seçin.";
                return View(model);
            }

            //eposta daha önce alınmış mı
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Hata = "Bu e-posta adresiyle zaten kayıtlı bir hesap var!";
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

            //İsim Kuralı (Sadece harf ve boşluk, 2-50 karakter)
            if (!_kurallar.IsimKontrol(tempUser))
            {
                ViewBag.Hata = "İsim sadece harflerden oluşmalı ve 2-50 karakter arasında olmalıdır!";
                return View(model);
            }

            //sifre Kuralı (8-12 karakter, en az 1 büyük harf ve 1 özel karakter)
            if (!_kurallar.PasswordKontrol(tempUser))
            {
                ViewBag.Hata = "Şifre 8-12 karakter olmalı, en az 1 büyük harf ve 1 özel karakter içermelidir!";
                return View(model);
            }

            //eposta formatı kuralı (Regex)
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

            // İsimdeki fazla boşlukları temizle
            newUser.BoslukTemizle(newUser.Name);

            // Şifreyi BCrypt ile güvenle hashle
            newUser.SifreyiHashle(model.Password);

            // Veritabanına ekle ve kaydet
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Kayıt başarıl ogrenciyi doğrudan ogrenci giriş sayfasına yönlendir
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
            // kullanıcı adı kontolu
            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ViewBag.Hata = $"'{model.UserName}' kullanıcı adı zaten alınmış! Başka bir kullanıcı adı seçin.";
                return View(model);
            }
            // eposta kontrolu
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
            // kurallar kontrolu (İsim, Şifre, eposta)
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
            // admin kullanıcı olulturma (Rolü ROLE_ADMIN yapılır)
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
            // Başarılı kayıt sonrası Admin Giriş ekranına yönlendir
            return RedirectToAction("AdminLogin");
        }






        //ANA GİRİS
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

      
        //ogrenci giris islemleri
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

            // Kullanıcı adı veya eposta ile ara ve sadece ROLE_STUDENT olanı bul
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

            //sifre doğrulama BCrypt
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

            //giriş başarılı hataları sıfırla
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

     
        //admin giris işlemleri
      
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

            //kullanıcı adı veya eposta ile ara ve sadece ROLE_ADMIN olanı bul
            var user = _context.Users.FirstOrDefault(u =>
                (u.UserName == model.UsernameOrEmail || u.Email == model.UsernameOrEmail)
                && u.Role == "ROLE_ADMIN");

            if (user == null)
            {
                ViewBag.Hata = "Admin bilgileri veya şifre hatalı!";
                return View(model);
            }

            //hesap kilitli mi kontrolü (15 dakika)
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

            // sifre doğrulama BCrypt
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

            // giriş başarılı hataları sıfırla
            user.GirisSayisiSifirlama();
            _context.SaveChanges();

            // cookie ile oturum başlat
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

        

        // çıkış yap ve yerkisiz giriş
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