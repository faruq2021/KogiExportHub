using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KogiExportHub.Data;
using KogiExportHub.Services;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;
using System.Globalization;

namespace KogiExportHub;

public class Program
{
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        return WebApplication.CreateBuilder(args);
    }

    public static void Main(string[] args)
    {
        var builder = CreateBuilder(args);

// Configure localization for Nigerian culture
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-NG") };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-NG");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Flutterwave configuration
builder.Services.Configure<FlutterwaveSettings>(builder.Configuration.GetSection("Flutterwave"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentService, FlutterwavePaymentService>();

// Register cart service
builder.Services.AddScoped<ICartService, CartService>();

// Register email sender service
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Add HttpContextAccessor for dynamic URLs
builder.Services.AddHttpContextAccessor();

// Add these service registrations
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<EmailService>();

// Add this after AddDefaultIdentity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
});

// Add this line with other service registrations
builder.Services.AddScoped<ITaxService, TaxService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Add session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
DbInitializer.Initialize(context, roleManager).Wait();
}

app.Run();
    }
}
