using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Application.Services;
using CustomerSnapshot.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace CustomerSnapshot.Tests;

public class PersonalityProfileServiceTests
{
    [Fact]
    public async Task Builds_detailed_profile_based_on_interactions_and_quotes()
    {
        var interactions = new List<Interaction>
        {
            new() { InteractionDate = DateTime.UtcNow, Subject = new string('a', 80), InteractionType = "Email" },
            new() { InteractionDate = DateTime.UtcNow.AddDays(-2), Subject = "Short", InteractionType = "Call" }
        };
        var orders = new List<Order>
        {
            new() { OrderDate = DateTime.UtcNow.AddDays(-10), TotalAmount = 1200 },
            new() { OrderDate = DateTime.UtcNow.AddDays(-30), TotalAmount = 1300 }
        };
        var quotes = new List<Quote> { new() { Amount = 1000 }, new() { Amount = 900 } };

        var interactionRepo = new Mock<IInteractionRepository>();
        interactionRepo.Setup(r => r.GetRecentInteractionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(interactions);

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var quoteRepo = new Mock<IQuoteRepository>();
        quoteRepo.Setup(r => r.GetOpenQuotesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotes);

        var service = new PersonalityProfileService(interactionRepo.Object, orderRepo.Object, quoteRepo.Object);

        var profile = await service.BuildProfileAsync(1);

        profile.CommunicationPreference.Should().Be("Prefers detail");
        profile.ValueOrientation.Should().Be("Price-sensitive");
        profile.Notes.Should().Contain("Last spoke on");
    }
}
