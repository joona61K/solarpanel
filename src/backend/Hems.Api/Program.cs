using Hems.Api.Data;
using Hems.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HemsOptions>(builder.Configuration.GetSection("Hems"));

builder.Services.AddDbContext<HemsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddHttpClient("SolarService");
builder.Services.AddHttpClient("SpotPriceService");
builder.Services.AddHttpClient("WeatherService");
builder.Services.AddHttpClient();

builder.Services.AddScoped<ISpotPriceService, SpotPriceService>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddSingleton<SolarIntegrationService>();
builder.Services.AddSingleton<ISolarIntegrationService>(sp => sp.GetRequiredService<SolarIntegrationService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<SolarIntegrationService>());
builder.Services.AddSingleton<AutomationEngineService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AutomationEngineService>());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HEMS API", Version = "v1", Description = "Home Energy Management System API" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HemsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HEMS API v1"));
}

app.UseCors();
app.MapControllers();
app.Run();
