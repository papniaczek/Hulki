using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Hulki.Web.Data;
using Hulki.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. REJESTRACJA BAZY DANYCH (Musi być przed builder.Build()!)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. REJESTRACJA IDENTITY (System logowania i użytkowników)
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
// Dodajemy obsługę Razor Pages (potrzebne do wbudowanych widoków logowania)
builder.Services.AddRazorPages(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 3. WAŻNE: UseAuthentication musi być PRZED UseAuthorization!
app.UseAuthentication(); 
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 4. Mapowanie stron logowania i rejestracji
app.MapRazorPages(); 

app.Run();