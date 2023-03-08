using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using UdemyRabbitMQWeb.Excel.Hubs;
using UdemyRabbitMQWeb.Excel.Models;
using UdemyRabbitMQWeb.Excel.Services;
using UdemyRabbitMQWeb.Excel.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
   
});
builder.Services.Configure<RabbitMQSetting>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    HostName = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.Host,
    Port = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.Port,
    UserName = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.UserName,
    Password = sp.GetRequiredService<IOptions<RabbitMQSetting>>().Value.Password,
    DispatchConsumersAsync = true,
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>();
var app = builder.Build();

using var scope = app.Services.CreateScope();
var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
appDbContext.Database.MigrateAsync().Wait();
if (!appDbContext.Users.Any())
{
    userManager.CreateAsync(new() { UserName = "Sercan", Email = "sercan.uludag@hotmail.com" }, "myP@ssword123").Wait();
    userManager.CreateAsync(new() { UserName = "Sercan2", Email = "sercan2.uludag@hotmail.com" }, "myP@ssword123").Wait();
}


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
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MyHub>("/MyHub");
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
