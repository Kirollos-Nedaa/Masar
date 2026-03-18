using DotNetEnv;
using Masar.Core.IService;
using Masar.Core.Services;
using Masar.Domain.Models;
using Masar.Infrastructure.Constants;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Configure Database
var conn = Environment.GetEnvironmentVariable("CONN_STRING");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICandidateDashboardService, CandidateDashboardService>();

var app = builder.Build();

// Seed database
await SeedDatabaseAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.Run();

// =============================================
// Database Seeding
// =============================================
async Task SeedDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    try
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations automatically
        await context.Database.MigrateAsync();

        // Seed roles
        var roles = new[] { Roles.Admin, Roles.Candidate, Roles.Company };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred while seeding database.");
    }
}