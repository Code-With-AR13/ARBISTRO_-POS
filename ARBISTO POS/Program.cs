using ARBISTO_POS.Controllers;
using ARBISTO_POS.Data;
using ARBISTO_POS.Models;
using ARBISTO_POS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// MVC + Global "must be authenticated" policy
// --------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// --------------------------------------------------
// CORS (REQUIRED for Flutter / Web / Mobile)
// --------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod(); // OPTIONS allowed
    });
});

// --------------------------------------------------
// Cookie Authentication
// --------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "ARAuth";
    });

// --------------------------------------------------
// Email service
// --------------------------------------------------
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();

// --------------------------------------------------
// Backup Settings
// --------------------------------------------------
builder.Services.Configure<BackupSettings>(
    builder.Configuration.GetSection("BackupSettings"));

// --------------------------------------------------
// Database
// --------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// --------------------------------------------------
// Seed First Admin (runs once)
// --------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.AppUsers.AnyAsync())
    {
        PasswordHasher.CreateHash("Admin@123", out var h, out var s);
        db.AppUsers.Add(new AppUser
        {
            FullName = "System Admin",
            Email = "admin@testing.com",
            Role = "Admin",
            IsActive = true,
            PasswordHash = h,
            PasswordSalt = s
        });
        await db.SaveChangesAsync();
    }
}

// --------------------------------------------------
// Middleware Pipeline (ORDER MATTERS)
// --------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔥 CORS MUST BE HERE
app.UseCors("AllowFlutter");

app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// Routes
// --------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --------------------------------------------------
app.Run();
