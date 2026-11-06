using System.Linq; // c# sorgulama kütüphanesi aşşağıda Any fonksiyonu kullandık
using Lab3_proje.Models.Entities; // user sınıfını kullanabilmek için 

namespace Lab3_proje.Data
{
    public static class DbSeeder
    {
        public static void SeedAdmin(AppDbContext context)
        {
            
            if (context.Users.Any(u => u.UserName == "admin")) // Visual stdio her açılıp kapandığında admin  UserName alamıycak 
            {
                return;
            }

            // Sadece Sistem Yöneticisi  Hesabı Oluşturuyoruz
            var adminUser = new User // kendi oluşturduğumuz sınıf 
            {
                Name = "Sistem Yoneticisi",
                UserName = "admin",
                Email = "admin@staj.edu.tr",
                Role = "ROLE_ADMIN" // Rolü kesinlikle Admin
            };

            // kendi yazdığımız fonksiyon Şifresini kuralımıza uygun yapıp hashle
            adminUser.SifreyiHashle("Admin123!");

            // Veritabanına ekle ve kaydet
            context.Users.Add(adminUser); // admin user verı tabanına ekle
            context.SaveChanges();
        }
    }
}