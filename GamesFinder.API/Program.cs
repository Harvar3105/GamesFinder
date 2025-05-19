using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GamesFinder;
using GamesFinder.Application;
using GamesFinder.DAL;
using GamesFinder.DAL.Repositories;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.Database);
});

var twp = new TokenValidationParameters
{
    RoleClaimType = ClaimTypes.Role,
    NameClaimType = JwtRegisteredClaimNames.UniqueName,
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration.GetValue<string>("Security:JWTIssuer"),
    ValidAudience = builder.Configuration.GetValue<string>("Security:JWTAudience"),
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
        builder.Configuration.GetValue<string>("Security:JWTSecret")!
    )),
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {options.TokenValidationParameters = twp; });

builder.Services.AddAuthorization();

builder.Services.AddSingleton(new SteamOptions(
    domainName: builder.Configuration.GetValue<string>("SteamApi:Name")!,
    apiKey: builder.Configuration.GetValue<string>("SteamApi:Key")!
));
builder.Services.AddSingleton<SteamJsonFetcher>();
builder.Services.AddSingleton<GameSteamAppIdFiner>();
builder.Services.AddScoped<ICrawler, SteamCrawler>();
builder.Services.AddScoped<IGameOfferRepository<GameOffer>, GameOfferRepository>();
builder.Services.AddScoped<IGameRepository<Game>, GameRepository>();



BsonSerializer.RegisterSerializer(typeof(ECurrency), new EnumSerializer<ECurrency>(BsonType.String));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();