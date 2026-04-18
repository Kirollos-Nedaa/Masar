using DotNetEnv;
using Masar.Core.IService;
using Masar.Core.Services;
using Masar.Domain.Models;
using Masar.Infrastructure.Constants;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────
var conn = Environment.GetEnvironmentVariable("CONN_STRING");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn, sqlOptions => sqlOptions.EnableRetryOnFailure()));

// ── Identity ───────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ── Authentication (Google) ───────────────────────────────
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });

// ── Session ────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = ".Masar.Session";
});

// ── MVC ────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── App services ───────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

// ── Seed database ──────────────────────────────────────────
await SeedDatabaseAsync(app.Services);

// ── Middleware pipeline ────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ── Database seeding ───────────────────────────────────────
async Task SeedDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    try
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        var roles = new[] { Roles.Admin, Roles.Candidate, Roles.Company };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred while seeding database.");
    }
}