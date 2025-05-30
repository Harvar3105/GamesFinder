using GamesFinder.DAL.Repositories;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;


namespace GamesFinder.Tests.Repositories;

public class GameOfferRepositoryTests : RepositoryTests<GameOfferRepository, GameOffer>
{
    public GameOfferRepositoryTests() : base("gameoffer", 
        (db, logger) => new GameOfferRepository(db, logger))
    {
    }
    
    
    [Fact]
    public async Task SaveAsync_Should_Insert_GameOffer()
    {
        // Arrange
        var offer = Generator.GenerateGameOffer();

        // Act
        var result = await _repository.SaveAsync(offer);
        var saved = await _repository.GetByIdAsync(offer.Id);

        // Assert
        Assert.True(result);
        Assert.NotNull(saved);
        Assert.Equal(offer.GameId, saved.GameId);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_GameOffer()
    {
        var offer = Generator.GenerateGameOffer();

        await _repository.SaveAsync(offer);

        var deleted = await _repository.DeleteAsync(offer.Id);
        var found = await _repository.GetByIdAsync(offer.Id);

        Assert.True(deleted);
        Assert.Null(found);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_Should_Upsert()
    {
        var id = Guid.NewGuid();
        var offer = Generator.GenerateGameOffer(id: id);
        
        offer = Generator.GeneratePriceRange(gameOffer: offer, currency: ECurrency.Eur, initial: 15.99m, current:15.99m);

        var inserted = await _repository.SaveOrUpdateAsync(offer);

        offer.Prices[ECurrency.Eur] = Generator.GeneratePriceRange(initial:5m, current:5m);
        var updated = await _repository.SaveOrUpdateAsync(offer);
        var modified = await _repository.GetByIdAsync(id);

        Assert.True(inserted);
        Assert.True(updated);
        Assert.Equal(5m, modified?.Prices[ECurrency.Eur].Current);
        Assert.Equal(5m, modified?.Prices[ECurrency.Eur].Initial);
    }

    [Fact]
    public async Task GetByVendor_Should_Return_Correct_Offers()
    {
        var vendor = EVendor.Steam;
        var offer1 = Generator.GenerateGameOffer(vendor: vendor);
        var offer2 = Generator.GenerateGameOffer(vendor: vendor);

        await _repository.SaveAsync(offer1);
        await _repository.SaveAsync(offer2);

        var epicOffers = await _repository.GetByVendorAsync(vendor);

        Assert.Equal(2, epicOffers.Count);
        Assert.All(epicOffers, o => Assert.Equal(vendor, o.Vendor));

    }
}