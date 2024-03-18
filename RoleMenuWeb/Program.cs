using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleMenuWeb;
using RoleMenuWeb.Data;
using RoleMenuWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IDataAccessService,DataAccessService>();
//builder.Services.AddScoped<MenuService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(AuthorizeAccess)); 
});

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "PermissionDenied",
    pattern: "Home/PermissionDenied",
    defaults: new { controller = "Home", action = "PermissionDenied" }
);

app.MapControllerRoute(
    name: "Login",
    pattern: "Account/Login",
    defaults: new { controller = "Account", action = "Login" }
);

app.MapControllerRoute(
    name: "LogOff",
    pattern: "Account/LogOff",
    defaults: new { controller = "Account", action = "LogOff" }
);

app.MapControllerRoute(
    name: "Manage",
    pattern: "Account/Manage",
    defaults: new { controller = "Account", action = "Manage" }
);

//using (var scope = app.Services.CreateScope())
//{
//    var menuService = scope.ServiceProvider.GetRequiredService<MenuService>();
//    menuService.AddMenuItems();
//}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.MapRazorPages();

app.Run();
