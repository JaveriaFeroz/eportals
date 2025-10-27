using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Services;
using ProcureToPay.Data;
using ProcureToPay.Extensions;
using ProcureToPay.Services;
using ProcureToPay.Utilities;


ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Email and Password Generator Services
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, OutlookEmailService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
// Add Identity services
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddHttpContextAccessor();
// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Account/Login";
    options.LogoutPath = "/Auth/Account/Logout";
    options.AccessDeniedPath = "/Auth/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Register your custom services
builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddApplicationServices();

builder.Services.AddScoped<NavigationHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseHttpsRedirection();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    await next();
});


app.UsePathBase("/p2p");

app.UseStaticFiles();
app.UseRouting();

// Authentication and Authorization middleware (ORDER IS IMPORTANT)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// If you want to redirect root to Auth/Account/Login
//app.MapGet("/", (HttpContext context) =>
//{
//    if (context.User.Identity.IsAuthenticated)
//    {
//        return Results.Redirect("/Home/Index");
//    }
//    else
//    {
//        return Results.Redirect("/Auth/Account/Login");
//    }
//});
app.MapGet("/", (HttpContext context) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        return Results.LocalRedirect("~/Home/Index");
    }
    else
    {
        return Results.LocalRedirect("~/Auth/Account/Login");
    }
});
// Seed data BEFORE app.Run()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    // Ensure database is created and run migrations
    await context.Database.MigrateAsync();

    // Seed all data in proper order

}

app.Run();
