using Lab3_proje.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Lab3_proje.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }




        [HttpGet]
        public IActionResult OgrenciLogin()
        {
            return View(new OgrenciLoginViewModel());
        }

        [HttpPost]
        public IActionResult OgrenciLogin(OgrenciLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Öğrenci doğrulama buraya gelecek

            return View(model);
        }






    }
}