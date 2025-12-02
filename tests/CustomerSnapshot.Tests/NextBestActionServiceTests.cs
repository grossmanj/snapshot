using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Application.Services;
using CustomerSnapshot.Domain.Models;
using FluentAssertions;
using Moq;

namespace CustomerSnapshot.Tests;

public class NextBestActionServiceTests
{
    [Fact]
    public async Task Returns_overdue_invoice_action_when_invoices_are_late()
    {
        var invoices = new List<Invoice>
        {
            new() { InvoiceNumber = "INV-1", InvoiceDate = DateTime.UtcNow.AddDays(-40), DueDate = DateTime.UtcNow.AddDays(-5) }
        };

        var service = BuildService(invoices: invoices);

        var action = await service.DetermineNextActionAsync(1);

        action.Label.Should().Contain("overdue invoice");
    }

    [Fact]
    public async Task Returns_quote_follow_up_when_quotes_exist_and_no_blockers()
    {
        var quotes = new List<Quote> { new() { QuoteNumber = "Q-1", QuoteDate = DateTime.UtcNow.AddDays(-2), Amount = 1000 } };

        var service = BuildService(quotes: quotes);

        var action = await service.DetermineNextActionAsync(1);

        action.Label.Should().Contain("Follow up on quote");
    }

    private static NextBestActionService BuildService(
        IEnumerable<Invoice>? invoices = null,
        IEnumerable<Quote>? quotes = null,
        IEnumerable<Issue>? issues = null,
        IEnumerable<Interaction>? interactions = null,
        IEnumerable<Order>? orders = null)
    {
        invoices ??= Array.Empty<Invoice>();
        quotes ??= Array.Empty<Quote>();
        issues ??= Array.Empty<Issue>();
        interactions ??= Array.Empty<Interaction>();
        orders ??= Array.Empty<Order>();

        var invoiceRepo = new Mock<IInvoiceRepository>();
        invoiceRepo.Setup(r => r.GetInvoicesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(invoices.ToList());

        var quoteRepo = new Mock<IQuoteRepository>();
        quoteRepo.Setup(r => r.GetOpenQuotesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(quotes.ToList());

        var issueRepo = new Mock<IIssueRepository>();
        issueRepo.Setup(r => r.GetOpenIssuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(issues.ToList());

        var interactionRepo = new Mock<IInteractionRepository>();
        interactionRepo.Setup(r => r.GetRecentInteractionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(interactions.ToList());

        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(r => r.GetOrdersAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).ReturnsAsync(orders.ToList());

        return new NextBestActionService(invoiceRepo.Object, quoteRepo.Object, issueRepo.Object, interactionRepo.Object, orderRepo.Object);
    }
}
