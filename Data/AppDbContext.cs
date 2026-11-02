using Microsoft.EntityFrameworkCore;// Entity Framework kütüphanesini çağırır Db context ve dbset yapılarını bu sayede çağırız
using Lab3_proje.Models.Entities; // user.cs dosyasını bu dosyaya tanıtır

namespace Lab3_proje.Data  // Bu dosyanın Lab3_proje projesinin Data klasörüne ait olduğunu belirten adres etiketidir.
{
    // public class AppDbContext veriTabanın yönetim merkezi onun  alt sınıfndan kalıtmlanmış alt sınıfını Db context kullanıyoruz
    public class AppDbContext : DbContext //kalıtım
    {
       
        //constructor metot
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanındaki fiziksel Users tablomuz
        public DbSet<User> Users { get; set; }
    }
}