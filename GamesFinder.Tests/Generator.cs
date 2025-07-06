using System.Text;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;

namespace GamesFinder.Tests;

public static class Generator
{
    private readonly static Random _random = new();

    public static GameOffer GenerateGameOffer(
        Guid? id = null,
        Guid? gameId = null,
        bool available = false,
        EVendor? vendor = null,
        string? vendorsUrl = null
        )
    {
        var obj = new GameOffer(
            gameId: gameId ?? Guid.NewGuid(),
            vendor: vendor ?? (EVendor)_random.Next(0, 3),
            vendorsGameId: (gameId ?? Guid.NewGuid()).ToString(),
            vendorsUrl: vendorsUrl ?? GetRandomString(20),
            prices: new Dictionary<ECurrency, GameOffer.PriceRange>(),
            available: available
            );

        if (id != null)
        {
            obj.Id = (Guid)id;
        }
        
        return obj;
    }

    public static GameOffer.PriceRange GeneratePriceRange(
        decimal? initial = null,
        bool forceInitialNull = false,
        decimal? current = null,
        bool forceCurrentNull = false
        )
    {
        return new GameOffer.PriceRange(
            initial: forceInitialNull ? null : initial ?? GeneratePrice(),
            current: forceCurrentNull ? null : current ?? GeneratePrice()
            );
    }

    public static GameOffer GeneratePriceRange(
        GameOffer gameOffer,
        ECurrency? currency = null,
        decimal? initial = null,
        bool forceInitialNull = false,
        decimal? current = null,
        bool forceCurrentNull = false
        )
    {
        var priceRange = GeneratePriceRange(initial, forceInitialNull, current, forceCurrentNull);
        
        gameOffer.Prices.Add(currency ?? (ECurrency) _random.Next(0, 2), priceRange);

        return gameOffer;
    }

    public static string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            result.Append(chars[_random.Next(chars.Length)]);
        }

        return result.ToString();
    }
    
    public static decimal GeneratePrice()
    {
        int wholePart = _random.Next(0, 1000);
        int fractionPart = _random.Next(0, 100);

        decimal result = wholePart + fractionPart / 100m;
        return result;
    }
}