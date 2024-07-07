using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;
using UroMeter.Web.Extensions;
using UroMeter.Web.Settings;
using UroMeter.Web.Startup;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

// Database
var connectionString = configuration.GetConnectionString("AppDatabase") ?? throw new ArgumentNullException("ConnectionStrings:AppDatabase", "Database connection string is not initialized");
services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
services.AddAsyncInitializer<DatabaseInitializer>();

// Logging
builder.Logging.AddJsonConsole(config =>
{
    config.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
});

// MQTT
var brokerHostSettings = new BrokerHostSettings();
configuration.GetSection(nameof(BrokerHostSettings)).Bind(brokerHostSettings);
AppSettingsProvider.BrokerHostSettings = brokerHostSettings;

var clientSettings = new ClientSettings();
configuration.GetSection(nameof(ClientSettings)).Bind(clientSettings);
AppSettingsProvider.ClientSettings = clientSettings;

services.AddMqttClientHostedService();

// Services.
services.AddControllersWithViews();
services.AddSwaggerGen();

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

app.Run();
