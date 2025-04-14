using Microsoft.EntityFrameworkCore;
using TelegramCards.Managers.Implementations;
using TelegramCards.Managers.Interfaces;
using TelegramCards.Models;
using TelegramCards.Models.DTO;
using TelegramCards.Repositories.implementations;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.Implementations;
using TelegramCards.Services.interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

var minioSecretsPath = Path.Combine(Directory.GetCurrentDirectory(), "secrets_config.json");
if (File.Exists(minioSecretsPath))
{
    builder.Configuration.AddJsonFile(minioSecretsPath, optional: true);
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<IUserRepository, EfCoreUserRepository>();
builder.Services.AddScoped<ICardRepository, EfCoreCardRepository>();
builder.Services.AddScoped<ICardBaseRepository, EfCoreCardBaseRepository>();

builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ICardManager, CardManager>();
builder.Services.AddScoped<ICardBaseManager, CardBaseManager>();

builder.Services.AddSingleton<IFileDriveService, S3DriveService>();
builder.Services.AddSingleton<ICardBaseGeneratorService, CardBaseGeneratorService>(provider => 
{
    var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
    int stackSize = Convert.ToInt32(builder.Configuration.GetConnectionString("CardSettings:GenerateLengthStack"));
    var config = provider.GetRequiredService<IConfiguration>();
    
    var rarityConfig = builder.Configuration.GetSection("RarityDistribution");
    var total = rarityConfig.GetChildren().Sum(x => x.Get<double>());
    if (Math.Abs(total - 100.0) > 0.01)
    {
        throw new Exception("Rarity distribution must sum to 100%");
    }
    return new CardBaseGeneratorService(scopeFactory, stackSize, config);
});


builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteData"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();