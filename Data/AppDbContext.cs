using Microsoft.EntityFrameworkCore;// Entity Framework kütüphanesini çağırır Db context ve dbset yapılarını bu sayede çağırız
using Lab3_proje.Models.Entities; 
namespace Lab3_proje.Data  // Bu dosyanın Lab3_proje projesinin Data klasörüne ait olduğunu belirten adres etiketidir.
{
    // public class AppDbContext veriTabanın yönetim merkezi onun  alt sınıfndan kalıtmlanmış alt sınıfını Db context kullanıyoruz
    public class AppDbContext : DbContext 
    {
       
        //constructor metot
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanındaki fiziksel Users tablomuz
        public DbSet<User> Users { get; set; }
        public DbSet<OgrenciDetay> OgrenciDetaylari { get; set; }
        public DbSet<StajBasvuru> StajBasvurulari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User ve OgrenciDetay arasındaki ilişki
            modelBuilder.Entity<User>()
                .HasOne(u => u.OgrenciDetay)
                .WithOne(od => od.User)
                .HasForeignKey<OgrenciDetay>(od => od.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ve StajBasvuru arasındaki  İlişki (Bir öğrencinin birden çok staj başvurusu olabilir)
            modelBuilder.Entity<StajBasvuru>()
                .HasOne(sb => sb.User)
                .WithMany()
                .HasForeignKey(sb => sb.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}