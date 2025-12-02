using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using System.Linq;

namespace CustomerSnapshot.Application.Services;

public class TalkingPointsService : ITalkingPointsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly IInteractionRepository _interactionRepository;

    public TalkingPointsService(
        IOrderRepository orderRepository,
        IInvoiceRepository invoiceRepository,
        IQuoteRepository quoteRepository,
        IIssueRepository issueRepository,
        IInteractionRepository interactionRepository)
    {
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _quoteRepository = quoteRepository;
        _issueRepository = issueRepository;
        _interactionRepository = interactionRepository;
    }

    public async Task<IReadOnlyList<TalkingPoint>> BuildTalkingPointsAsync(int customerId, PersonalityProfile profile, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrdersAsync(customerId, DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow, cancellationToken);
        var invoices = await _invoiceRepository.GetInvoicesAsync(customerId, cancellationToken);
        var quotes = await _quoteRepository.GetOpenQuotesAsync(customerId, 3, cancellationToken);
        var issues = await _issueRepository.GetOpenIssuesAsync(customerId, 3, cancellationToken);
        var interactions = await _interactionRepository.GetRecentInteractionsAsync(customerId, 5, cancellationToken);

        var talkingPoints = new List<TalkingPoint>();

        var recentOrder = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault();
        if (recentOrder is not null)
        {
            talkingPoints.Add(new TalkingPoint
            {
                Title = "Lead with what worked",
                Detail = $"They spent {recentOrder.TotalAmount:C0} on {recentOrder.OrderDate:d}. Reference it to anchor value."
            });
        }

        if (quotes.Any())
        {
            var topQuote = quotes.OrderByDescending(q => q.QuoteDate).First();
            talkingPoints.Add(new TalkingPoint
            {
                Title = "Nudge open quote",
                Detail = $"Quote {topQuote.QuoteNumber} for {topQuote.Amount:C0} is open since {topQuote.QuoteDate:d}. Offer next step."
            });
        }

        var overdue = invoices.Where(i => i.IsOverdue).ToList();
        if (overdue.Any())
        {
            talkingPoints.Add(new TalkingPoint
            {
                Title = "Acknowledge payment friction",
                Detail = $"{overdue.Count} invoice(s) overdue; ask if anything is blocking payment and propose a plan."
            });
        }

        if (issues.Any())
        {
            var critical = issues.FirstOrDefault(i => i.Severity.Equals("High", StringComparison.OrdinalIgnoreCase));
            talkingPoints.Add(new TalkingPoint
            {
                Title = critical is not null ? "Address high-severity issue" : "Close the loop on support",
                Detail = critical is not null
                    ? $"Issue '{critical.Summary}' is high severity ({critical.Status}). Confirm mitigation before selling."
                    : $"{issues.Count} open issue(s); provide status and expected resolution dates."
            });
        }

        if (!talkingPoints.Any())
        {
            talkingPoints.Add(new TalkingPoint
            {
                Title = "Check-in",
                Detail = "No blockers found. Share a quick win or relevant product update tailored to their segment."
            });
        }

        if (!string.IsNullOrWhiteSpace(profile.CommunicationPreference))
        {
            talkingPoints.Add(new TalkingPoint
            {
                Title = "Match communication style",
                Detail = profile.CommunicationPreference == "Keeps it brief"
                    ? "Keep the agenda to 3 bullets with clear next steps."
                    : "Send a one-page summary with appendix-level details for their review."
            });
        }

        var lastInteraction = interactions.OrderByDescending(i => i.InteractionDate).FirstOrDefault();
        if (lastInteraction is not null)
        {
            talkingPoints.Add(new TalkingPoint
            {
                Title = "Reference last touch",
                Detail = $"Last {lastInteraction.InteractionType.ToLowerInvariant()} on {lastInteraction.InteractionDate:g}: '{Truncate(lastInteraction.Subject, 60)}'. Close that loop."
            });
        }

        return talkingPoints.Take(5).ToList();
    }

    private static string Truncate(string value, int length)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= length)
        {
            return value;
        }

        return value.Substring(0, length) + "…";
    }
}
