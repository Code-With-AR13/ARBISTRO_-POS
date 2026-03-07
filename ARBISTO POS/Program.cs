using ARBISTO_POS.Controllers;
using ARBISTO_POS.Data;
using ARBISTO_POS.Hubs;
using ARBISTO_POS.Models;
using ARBISTO_POS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
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

// Add to ConfigureServices (services section)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(_ => true);
        });
}); ;

// --------------------------------------------------
// Register SignalR
// --------------------------------------------------
builder.Services.AddSignalR();
// ✅ This makes Clients.User("123") work using ClaimTypes.NameIdentifier
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();
// --------------------------------------------------
// Register AutoDelete Service
// --------------------------------------------------
builder.Services.AddHostedService<NotificationCleanupService>();
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

app.UseRouting();
app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");  // Added for cache
    }
});


// Keep after StaticFiles

app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// Routes
// --------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --------------------------------------------------


// --------------------------------------------------
// Map SignalR hubs
// --------------------------------------------------
app.MapHub<NotificationHub>("/notificationHub");

// --------------------------------------------------

app.Run();
