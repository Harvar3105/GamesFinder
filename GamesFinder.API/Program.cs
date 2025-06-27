using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GamesFinder;
using GamesFinder.Application;
using GamesFinder.Application.Crawlers;
using GamesFinder.Application.Services;
using GamesFinder.DAL;
using GamesFinder.DAL.Repositories;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
    NameClaimType = ClaimTypes.Name,
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
builder.Services.AddSingleton<GameSteamAppIdFinder>();

builder.Services.AddScoped<SteamCrawler>();
builder.Services.AddScoped<InstantGamingCrawler>();

builder.Services.AddScoped<IGameOfferRepository<GameOffer>, GameOfferRepository>();
builder.Services.AddScoped<IGameRepository<Game>, GameRepository>();
builder.Services.AddScoped<IUnprocessedGamesRepository<UnprocessedGame>, UnprocessedGamesRepository>();
builder.Services.AddScoped<IUserDataRepository, UserDataRepository>();

builder.Services.AddScoped<GamesWithOffersService>();



BsonSerializer.RegisterSerializer(typeof(ECurrency), new EnumSerializer<ECurrency>(BsonType.String));

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(30));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;

    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Incoming Request Body: {Body}", body);

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();