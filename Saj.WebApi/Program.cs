using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using StackExchange.Redis;
using Microsoft.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddDbContext<HouseDbContext>();
builder.Services.AddScoped<IHouseRepository, HouseRepository>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd");
builder.Services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Value.ToString()));

var keyVaultUrl = builder.Configuration.GetSection("KeyVault:KeyVaultUrl");
var keyVaultClientId = builder.Configuration.GetSection("KeyVault:ClientId");
var keyVaultClientSecret = builder.Configuration.GetSection("KeyVault:ClientSecret");
var keyVaultDirectoryId = builder.Configuration.GetSection("KeyVault:DirectoryId");

var credential = new ClientSecretCredential(keyVaultDirectoryId.Value.ToString(), keyVaultClientId.Value.ToString(), keyVaultClientSecret.Value.ToString());

var client = new SecretClient(new Uri(keyVaultUrl.Value.ToString()), credential);

//var client = new SecretClient(new Uri(keyVaultUrl.Value.ToString()), new DefaultAzureCredential());

//var secret = await client.GetSecretAsync("mypassword");

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors(p => p.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
