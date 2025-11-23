using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Bookify.Data;
using Bookify.Data.Models;
using Bookify.Data.UnitOfWork;
using Bookify.Services.Interfaces;
using Bookify.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/bookify-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Data Protection (for Session and Authentication cookies)
// This fixes the session cookie decryption warnings
builder.Services.AddDataProtection()
    .SetApplicationName("Bookify")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Register Unit of Work and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Configure Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Configure Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database with roles and initial data
await SeedDatabaseAsync(app);

async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Apply migrations
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
        
        // Seed roles
        await SeedRolesAsync(roleManager);
        
        // Seed admin user
        await SeedAdminUserAsync(userManager);
        
        // Seed initial data
        await SeedInitialDataAsync(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Helper methods for seeding
async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Customer" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
{
    const string adminEmail = "admin@bookify.com";
    const string adminPassword = "Admin@123";
    
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Log.Information("Admin user created: {Email}", adminEmail);
        }
        else
        {
            Log.Error("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

async Task SeedInitialDataAsync(ApplicationDbContext context)
{
    if (!context.RoomTypes.Any())
    {
        var roomTypes = new[]
        {
            new Bookify.Data.Models.RoomType
            {
                Name = "Standard Room",
                Description = "Comfortable standard room with all basic amenities",
                PricePerNight = 99.99m,
                MaxOccupancy = 2,
                IsActive = true
            },
            new Bookify.Data.Models.RoomType
            {
                Name = "Deluxe Room",
                Description = "Spacious deluxe room with premium amenities",
                PricePerNight = 149.99m,
                MaxOccupancy = 3,
                IsActive = true
            },
            new Bookify.Data.Models.RoomType
            {
                Name = "Suite",
                Description = "Luxurious suite with separate living area",
                PricePerNight = 249.99m,
                MaxOccupancy = 4,
                IsActive = true
            }
        };
        
        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();
        
        // Add some sample rooms
        var rooms = new[]
        {
            new Bookify.Data.Models.Room { RoomNumber = "101", RoomTypeId = 1, IsAvailable = true },
            new Bookify.Data.Models.Room { RoomNumber = "102", RoomTypeId = 1, IsAvailable = true },
            new Bookify.Data.Models.Room { RoomNumber = "201", RoomTypeId = 2, IsAvailable = true },
            new Bookify.Data.Models.Room { RoomNumber = "202", RoomTypeId = 2, IsAvailable = true },
            new Bookify.Data.Models.Room { RoomNumber = "301", RoomTypeId = 3, IsAvailable = true },
            new Bookify.Data.Models.Room { RoomNumber = "302", RoomTypeId = 3, IsAvailable = true }
        };
        
        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
    }
}

// Stripe Settings class
public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
