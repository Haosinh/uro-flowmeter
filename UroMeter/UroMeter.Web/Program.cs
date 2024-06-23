using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.Web.Mqtt;
using UroMeter.Web.Startup;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("AppDatabase") ?? throw new ArgumentNullException("ConnectionStrings:AppDatabase", "Database connection string is not initialized");

services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
services.AddAsyncInitializer<DatabaseInitializer>();

// Add services to the container.
services.AddControllersWithViews();
services.AddSwaggerGen();

services.AddSingleton<MqttService>();

var app = builder.Build();

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

app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

var mqttService = scope.ServiceProvider.GetRequiredService<MqttService>();
var mqttBroker = configuration.GetValue<string>("Mqtt:Broker") ?? throw new ArgumentNullException("Mqtt:Broker", "Mqtt broker is not initialized");
var mqttUsername = configuration.GetValue<string>("Mqtt:Username") ?? throw new ArgumentNullException("Mqtt:Username", "Mqtt username is not initialized");
var mqttPassword = configuration.GetValue<string>("Mqtt:Password") ?? throw new ArgumentNullException("Mqtt:Password", "Mqtt password is not initialized");
await mqttService.Setup(mqttBroker, mqttUsername, mqttPassword);

app.Run();
