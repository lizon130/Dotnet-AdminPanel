using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Drawing;  // Add this
using System.Drawing.Imaging;  // Add this

var builder = WebApplication.CreateBuilder(args);

// Create default avatar if it doesn't exist
try
{
    var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    var imagesPath = Path.Combine(wwwrootPath, "images");

    if (!Directory.Exists(imagesPath))
    {
        Directory.CreateDirectory(imagesPath);
    }

    var defaultAvatarPath = Path.Combine(imagesPath, "default-avatar.png");
    if (!System.IO.File.Exists(defaultAvatarPath))
    {
        // Create a simple colored circle as default avatar
        using (var bitmap = new Bitmap(200, 200))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.LightGray);
            graphics.FillEllipse(Brushes.SteelBlue, 10, 10, 180, 180);
            graphics.DrawString("U", new Font("Arial", 80, FontStyle.Bold),
                Brushes.White, 60, 50);
            bitmap.Save(defaultAvatarPath, ImageFormat.Png);
        }
        Console.WriteLine("Default avatar created successfully.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not create default avatar: {ex.Message}");
    // Don't crash the app if avatar creation fails
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT ORDER
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();