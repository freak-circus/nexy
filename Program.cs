using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddLogging(logging =>
{
    logging.AddSerilog(new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .Filter.ByIncludingOnly(evt => 
            evt.MessageTemplate.Text.Contains("CampaignTracking") || 
            evt.MessageTemplate.Text.Contains("ConversionTracking") || 
            evt.MessageTemplate.Text.Contains("TrackClick") || 
            evt.Level == Serilog.Events.LogEventLevel.Warning || 
            evt.Level == Serilog.Events.LogEventLevel.Error)
        .WriteTo.File(new JsonFormatter(), "logs/campaigns.json", rollingInterval: RollingInterval.Day)
        .WriteTo.Console()
        .CreateLogger());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Назначение роли "Admin" пользователю tony@stark.com
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var roleName = "Admin";
    var roleExists = await roleManager.RoleExistsAsync(roleName);
    if (!roleExists)
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    var user = await userManager.FindByEmailAsync("tony@stark.com");
    if (user != null && !await userManager.IsInRoleAsync(user, roleName))
    {
        await userManager.AddToRoleAsync(user, roleName);
        Console.WriteLine("Роль 'Admin' успешно выдана пользователю tony@stark.com");
    }
    else if (user == null)
    {
        Console.WriteLine("Пользователь tony@stark.com не найден");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();