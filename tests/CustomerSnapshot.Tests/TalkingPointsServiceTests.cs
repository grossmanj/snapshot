using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Application.Services;
using CustomerSnapshot.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace CustomerSnapshot.Tests;

public class TalkingPointsServiceTests
{
    [Fact]
    public async Task Builds_talking_points_from_orders_and_profile()
    {
        var orders = new List<Order> { new() { OrderDate = DateTime.UtcNow.AddDays(-3), TotalAmount = 5000 } };
        var invoices = new List<Invoice>();
        var quotes = new List<Quote> { new() { QuoteNumber = "Q-10", QuoteDate = DateTime.UtcNow.AddDays(-1), Amount = 2000, Description = "Test" } };
        var issues = new List<Issue>();
        var interactions = new List<Interaction>();
        var profile = new PersonalityProfile { CommunicationPreference = "Keeps it brief" };

        var service = BuildService(orders, invoices, quotes, issues, interactions);

        var points = await service.BuildTalkingPointsAsync(1, profile);

        points.Should().NotBeEmpty();
        points.Should().Contain(p => p.Title.Contains("Lead with what worked"));
        points.Should().Contain(p => p.Title.Contains("Match communication style"));
    }

    private static TalkingPointsService BuildService(
        IEnumerable<Order> orders,
        IEnumerable<Invoice> invoices,
        IEnumerable<Quote> quotes,
        IEnumerable<Issue> issues,
        IEnumerable<Interaction> interactions)
    {
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders.ToList());

        var invoiceRepo = new Mock<IInvoiceRepository>();
        invoiceRepo.Setup(r => r.GetInvoicesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(invoices.ToList());

        var quoteRepo = new Mock<IQuoteRepository>();
        quoteRepo.Setup(r => r.GetOpenQuotesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(quotes.ToList());

        var issueRepo = new Mock<IIssueRepository>();
        issueRepo.Setup(r => r.GetOpenIssuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(issues.ToList());

        var interactionRepo = new Mock<IInteractionRepository>();
        interactionRepo.Setup(r => r.GetRecentInteractionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(interactions.ToList());

        return new TalkingPointsService(orderRepo.Object, invoiceRepo.Object, quoteRepo.Object, issueRepo.Object, interactionRepo.Object);
    }
}
