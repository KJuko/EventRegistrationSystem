using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
var runMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");
var seedDemoDataOnStartup = builder.Configuration.GetValue<bool>("Database:SeedDemoDataOnStartup");
var isEntityFrameworkDesignTime = AppDomain.CurrentDomain.GetAssemblies()
    .Any(assembly => assembly.GetName().Name == "Microsoft.EntityFrameworkCore.Design");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddIdentity<EventUser, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<EventDbContext>()
    .AddClaimsPrincipalFactory<EventUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".event-registration.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(5);
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageEvents", policy =>
        policy.RequireRole(RoleNames.Organizer, RoleNames.SuperAdmin));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(RoleNames.SuperAdmin));
});
builder.Services.AddScoped<IEventRepository, EfEventRepository>();
builder.Services.AddSingleton<IAssetStorageService, LocalAssetStorageService>();

var app = builder.Build();

if (!isEntityFrameworkDesignTime && (runMigrationsOnStartup || seedDemoDataOnStartup))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();

    if (runMigrationsOnStartup)
    {
        context.Database.Migrate();
    }

    if (seedDemoDataOnStartup)
    {
        await EventDbSeeder.SeedDemoDataAsync(scope.ServiceProvider);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
