using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using ToplulukYonetimSistemi.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")));
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("ToplulukCookie")
    .AddCookie("ToplulukCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(4);
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Veritabani migration islemi basarisiz oldu. Uygulama acilmaya devam edecek.");
    }

    try
    {
        await DatabaseRepair.EnsureMemberCommunitySchemaAsync(dbContext);
        await DatabaseRepair.EnsureContactMessageSchemaAsync(dbContext);
        await DatabaseRepair.EnsureMediaColumnsAsync(dbContext);
        await DatabaseRepair.EnsureAnnouncementMediaSchemaAsync(dbContext);
        await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(dbContext);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Uye-topluluk veritabani kontrolu basarisiz oldu.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
