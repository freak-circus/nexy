using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // ⬅️ добавлено
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// 🔐 Назначение роли "Admin" пользователю tony@stark.com
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var roleName = "Admin";

    // Создаём роль Admin, если её ещё нет
    var roleExists = await roleManager.RoleExistsAsync(roleName);
    if (!roleExists)
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    // Назначаем роль пользователю
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

app.UseAuthentication(); // ⬅️ обязательно!
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
