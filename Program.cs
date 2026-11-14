using Microsoft.EntityFrameworkCore;
using Lab3_proje.Data;
using Microsoft.AspNetCore.Authentication.Cookies; //

var builder = WebApplication.CreateBuilder(args);

// SQLite Veritabanı Bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();


// Cookie Tabanlı Kimlik Doğrulama Servisi
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) //kullanıcının giriş yapacağı siteyi kontrol aldık
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";               // Giriş yapmamış kişi buraya yönlendirilir
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi olmayan kişi (403 durumu) buraya yönlendirilir
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // 60 dakika hareketsiz kalırsa oturum kapanacak
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); //yetki kontrol  verilerin hangi kullanıcıdan geldiğini kontrol eder 
app.UseAuthorization();  // 2.kontrol kapısı bu adımdan sonra gelen veriler ile o kişinin tipine konrol edecek ROLE_Student



app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();



// Proje başlarken veritabanında Admin yoksa oluştur
using (var scope = app.Services.CreateScope()) //
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.SeedAdmin(context);
}
 app.Run();