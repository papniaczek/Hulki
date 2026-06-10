using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Warstwa serwisów (użyta m.in. przez publiczne API sklepu).
builder.Services.AddScoped<IShopService, ShopService>();

// Serwis cytatów: named HttpClient + in-memory cache, żeby nie bić w zewnętrzne API przy każdym żądaniu.
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient(QuoteService.QuotesClientName, client =>
{
    client.BaseAddress = new Uri("https://zenquotes.io/api/");
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "Hulki.Web/1.0");
});
// Tłumacz EN -> PL, żeby cytaty wyświetlały się po polsku. MyMemory działa bez klucza
// (limit 5000 znaków/dzień anonimowo, 50 000 po podaniu e-maila).
builder.Services.AddHttpClient(QuoteService.TranslationClientName, client =>
{
    client.BaseAddress = new Uri("https://api.mymemory.translated.net/");
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "Hulki.Web/1.0");
});
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
var app = builder.Build();

// SEEDING ADMINA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Dociąga zaległe migracje (np. AddShopItemFields), żeby kolumny Description / IconPath / Price
        // istniały w bazie zanim API sklepu zacznie z nich czytać.
        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = "admin@admin.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Adminowski",
                EmailConfirmed = true
            };

            var createAdmin = await userManager.CreateAsync(admin, "admin123");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");

                if (!context.Wallets.Any(w => w.AppUserId == admin.Id))
                {
                    context.Wallets.Add(new Wallet { AppUserId = admin.Id, Balance = 9999 });
                    await context.SaveChangesAsync();
                }
            }
        }

        // SEEDING SKLEPU – uruchamiany przy starcie, dzięki czemu /api/shop/items
        // od razu zwraca kompletne dane (bez konieczności wchodzenia na /Store).
        await StoreDataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Błąd podczas inicjalizacji danych startowych.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Routing atrybutowy dla kontrolerów API (np. /api/shop/items).
app.MapControllers();
app.MapRazorPages();

app.Run();