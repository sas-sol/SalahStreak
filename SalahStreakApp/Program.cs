using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Services;
using SalahStreakApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Add BioTime API services
builder.Services.AddHttpClient<BioTimeApiService>();
builder.Services.AddScoped<BioTimeApiService>();
builder.Services.AddHostedService<BiometricPollingService>();

// Add Attendance Scoring services
builder.Services.AddScoped<AttendanceScoringService>();
builder.Services.AddHostedService<ScoreProcessingService>();

// Add Round Management services
builder.Services.AddScoped<RoundManagementService>();
builder.Services.AddHostedService<RoundAutoManagementService>();

// Configure BioTime settings
builder.Services.Configure<BioTimeConfig>(
    builder.Configuration.GetSection("BioTimeApi"));

var app = builder.Build();

// 🌱 SEED THE DATABASE BEFORE RUNNING THE APP
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("🌱 Starting database seeding...");
        DbSeeder.Seed(context);
        Console.WriteLine("✅ Database seeding completed!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        Console.WriteLine($"❌ Error seeding database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
