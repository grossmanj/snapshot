using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using System.Linq;

namespace CustomerSnapshot.Application.Services;

public class NextBestActionService : INextBestActionService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly IInteractionRepository _interactionRepository;
    private readonly IOrderRepository _orderRepository;

    public NextBestActionService(
        IInvoiceRepository invoiceRepository,
        IQuoteRepository quoteRepository,
        IIssueRepository issueRepository,
        IInteractionRepository interactionRepository,
        IOrderRepository orderRepository)
    {
        _invoiceRepository = invoiceRepository;
        _quoteRepository = quoteRepository;
        _issueRepository = issueRepository;
        _interactionRepository = interactionRepository;
        _orderRepository = orderRepository;
    }

    public async Task<NextBestAction> DetermineNextActionAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetInvoicesAsync(customerId, cancellationToken);
        var quotes = await _quoteRepository.GetOpenQuotesAsync(customerId, 5, cancellationToken);
        var issues = await _issueRepository.GetOpenIssuesAsync(customerId, 5, cancellationToken);
        var interactions = await _interactionRepository.GetRecentInteractionsAsync(customerId, 5, cancellationToken);
        var orders = await _orderRepository.GetOrdersAsync(customerId, DateTime.UtcNow.AddMonths(-18), DateTime.UtcNow, cancellationToken);

        var overdueInvoices = invoices.Where(i => i.IsOverdue).OrderByDescending(i => i.DueDate).ToList();
        if (overdueInvoices.Any())
        {
            var overdue = overdueInvoices.First();
            return new NextBestAction
            {
                Label = "Check in about overdue invoice",
                Explanation = $"Invoice {overdue.InvoiceNumber} due {overdue.DueDate:d} remains unpaid. Offer help removing blockers."
            };
        }

        var criticalIssue = issues.FirstOrDefault(i => i.Severity.Equals("High", StringComparison.OrdinalIgnoreCase));
        if (criticalIssue is not null)
        {
            return new NextBestAction
            {
                Label = "Escalate support",
                Explanation = $"High severity issue '{criticalIssue.Summary}' is {criticalIssue.Status}. Resolve before pursuing new revenue."
            };
        }

        var freshestQuote = quotes.OrderByDescending(q => q.QuoteDate).FirstOrDefault();
        if (freshestQuote is not null)
        {
            return new NextBestAction
            {
                Label = $"Follow up on quote {freshestQuote.QuoteNumber}",
                Explanation = $"Quote from {freshestQuote.QuoteDate:d} worth {freshestQuote.Amount:C0}. Confirm decision path and close timing."
            };
        }

        var nextOrderPrompt = EvaluateReorderCadence(orders, interactions);
        if (nextOrderPrompt is not null)
        {
            return nextOrderPrompt;
        }

        return new NextBestAction
        {
            Label = "Call now — stay top of mind",
            Explanation = "No urgent blockers detected. A short value check-in keeps the account warm."
        };
    }

    private static NextBestAction? EvaluateReorderCadence(IReadOnlyCollection<Order> orders, IReadOnlyCollection<Interaction> interactions)
    {
        if (!orders.Any())
        {
            return null;
        }

        var ordered = orders.OrderBy(o => o.OrderDate).ToList();
        var gaps = new List<double>();
        for (var i = 1; i < ordered.Count; i++)
        {
            gaps.Add((ordered[i].OrderDate - ordered[i - 1].OrderDate).TotalDays);
        }

        var cadenceDays = gaps.Any() ? gaps.Average() : 45;
        var lastOrder = ordered.Last();
        var daysSinceLastOrder = (DateTime.UtcNow - lastOrder.OrderDate).TotalDays;

        if (daysSinceLastOrder > cadenceDays - 5)
        {
            var lastInteraction = interactions.OrderByDescending(i => i.InteractionDate).FirstOrDefault();
            var recencyNote = lastInteraction is null
                ? "No recent interactions on record."
                : $"Last spoke on {lastInteraction.InteractionDate:d}.";

            return new NextBestAction
            {
                Label = "Proactive reorder check",
                Explanation = $"Typical cadence {cadenceDays:N0} days; it's been {daysSinceLastOrder:N0}. {recencyNote} Call to capture the next cycle."
            };
        }

        return null;
    }
}
