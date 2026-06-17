using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Hulki.Web.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount  = false;
    options.Password.RequireDigit           = false;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
    options.Password.RequireLowercase       = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── ZABEZPIECZONE COOKIES ─────────────────────────────────────────────────────
// Dev:  SameAsRequest + Lax  → działa przez HTTP lokalnie
// Prod: Always       + Strict → wymaga HTTPS, blokuje CSRF
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name         = "Hulki.Auth";
    options.Cookie.HttpOnly     = true;   // JS nie może czytać cookie
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = builder.Environment.IsDevelopment()
        ? SameSiteMode.Lax
        : SameSiteMode.Strict;
    options.ExpireTimeSpan    = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath         = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.AccessDeniedPath  = "/Account/Login";
});

// Antiforgery – ta sama logika dev/prod
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name         = "Hulki.XSRF";
    options.Cookie.HttpOnly     = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = builder.Environment.IsDevelopment()
        ? SameSiteMode.Lax
        : SameSiteMode.Strict;
});

// ── Serwisy aplikacji ─────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITherapyGoalService, TherapyGoalService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();

// ── Serwis SQL (procedury / funkcje / triggery) ───────────────────────────────
builder.Services.AddScoped<SqlObjectsService>();

// ── Swagger / OpenAPI ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "Hulki API",
        Version     = "v1",
        Description = "REST API aplikacji Hulki – sklep, cytaty motywacyjne i statystyki użytkownika " +
                      "(statystyki wywołują procedury i funkcje składowane SQL Server)."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient(QuoteService.QuotesClientName, client =>
{
    client.BaseAddress = new Uri("https://zenquotes.io/api/");
    client.Timeout     = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "Hulki.Web/1.0");
});
builder.Services.AddHttpClient(QuoteService.TranslationClientName, client =>
{
    client.BaseAddress = new Uri("https://api.mymemory.translated.net/");
    client.Timeout     = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "Hulki.Web/1.0");
});
builder.Services.AddScoped<IQuoteService, QuoteService>();

var app = builder.Build();

// ── Inicjalizacja danych startowych ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services    = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context     = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminEmail = "admin@admin.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName       = adminEmail,
                Email          = adminEmail,
                FirstName      = "Admin",
                LastName       = "Adminowski",
                EmailConfirmed = true
            };
            var createAdmin = await userManager.CreateAsync(admin, "admin123");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                admin = null; // tworzenie się nie powiodło - nic więcej do naprawy
            }
        }

        if (admin != null)
        {
            if (!context.Wallets.Any(w => w.AppUserId == admin.Id))
            {
                context.Wallets.Add(new Wallet { AppUserId = admin.Id, Balance = 9999 });
                await context.SaveChangesAsync();
            }

            // Logowanie w tej aplikacji weryfikuje hasło względem CustomUsers
            // (AccountController.Login), nie względem AspNetUsers/Identity —
            // bez tego wpisu konto admina istnieje, ale nigdy nie zaloguje się
            // przez formularz logowania (zwraca "niepoprawny e-mail lub hasło").
            // Sprawdzenie działa też dla kont admina utworzonych przed tą poprawką.
            if (!await context.CustomUsers.AnyAsync(u => u.Email == adminEmail))
            {
                context.CustomUsers.Add(new CustomUser
                {
                    FirstName    = admin.FirstName,
                    LastName     = admin.LastName,
                    Email        = adminEmail,
                    PasswordHash = CustomPasswordHasher.Hash("admin123"),
                    IsTherapist  = false,
                    AspNetUserId = admin.Id
                });
                await context.SaveChangesAsync();
            }
        }

        await StoreDataSeeder.SeedAsync(context);

        if (app.Environment.IsDevelopment())
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Blad podczas inicjalizacji danych startowych.");
    }
}

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    // Swagger – dostępny tylko w środowisku deweloperskim
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hulki API v1");
        options.RoutePrefix = "swagger"; // -> /swagger
    });
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
app.MapControllers();
app.MapRazorPages();

app.Run();