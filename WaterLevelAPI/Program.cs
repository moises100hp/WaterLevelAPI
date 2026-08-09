using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Context;
using WaterLevelAPI.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IWaterLevelService, WaterLevelService>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS: necessário se o front (ou qualquer página no navegador) chamar a API de outro domínio
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Aplica as migrations pendentes (em vez de EnsureCreated, que ignora migrations
// e impede a evolução do schema no futuro)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
