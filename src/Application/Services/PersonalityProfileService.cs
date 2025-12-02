using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using System.Linq;

namespace CustomerSnapshot.Application.Services;

public class PersonalityProfileService : IPersonalityProfileService
{
    private readonly IInteractionRepository _interactionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IQuoteRepository _quoteRepository;

    public PersonalityProfileService(
        IInteractionRepository interactionRepository,
        IOrderRepository orderRepository,
        IQuoteRepository quoteRepository)
    {
        _interactionRepository = interactionRepository;
        _orderRepository = orderRepository;
        _quoteRepository = quoteRepository;
    }

    public async Task<PersonalityProfile> BuildProfileAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var interactions = await _interactionRepository.GetRecentInteractionsAsync(customerId, 25, cancellationToken);
        var orders = await _orderRepository.GetOrdersAsync(customerId, DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow, cancellationToken);
        var quotes = await _quoteRepository.GetOpenQuotesAsync(customerId, 10, cancellationToken);

        var averageSubjectLength = interactions.Any()
            ? interactions.Average(i => i.Subject.Length)
            : 0;

        var communicationPreference = averageSubjectLength > 45
            ? "Prefers detail"
            : "Keeps it brief";

        var averageOrder = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
        var quoteToOrderRatio = orders.Any() ? (double)quotes.Count / orders.Count : quotes.Count;
        var valueOrientation = quoteToOrderRatio > 0.6 || averageOrder < 500
            ? "Price-sensitive"
            : "Value-first";

        var responseCadence = CalculateResponseCadence(interactions);

        var notes = BuildNotes(interactions, orders, quotes, communicationPreference, valueOrientation, responseCadence);

        return new PersonalityProfile
        {
            CommunicationPreference = communicationPreference,
            ValueOrientation = valueOrientation,
            ResponseCadence = responseCadence,
            Notes = notes
        };
    }

    private static string CalculateResponseCadence(IReadOnlyCollection<Interaction> interactions)
    {
        if (!interactions.Any())
        {
            return "Unknown cadence";
        }

        var ordered = interactions
            .OrderByDescending(i => i.InteractionDate)
            .ToArray();

        var deltas = new List<double>();
        for (var i = 0; i < ordered.Length - 1; i++)
        {
            deltas.Add((ordered[i].InteractionDate - ordered[i + 1].InteractionDate).TotalDays);
        }

        var averageGap = deltas.Any() ? deltas.Average() : 14;

        return averageGap <= 5 ? "Fast responses" : averageGap <= 12 ? "Responds within a week" : "Slow responses";
    }

    private static string BuildNotes(
        IReadOnlyCollection<Interaction> interactions,
        IReadOnlyCollection<Order> orders,
        IReadOnlyCollection<Quote> quotes,
        string communicationPreference,
        string valueOrientation,
        string responseCadence)
    {
        var lastInteraction = interactions.OrderByDescending(i => i.InteractionDate).FirstOrDefault();
        var lastOrder = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault();

        var summary = new List<string>
        {
            communicationPreference,
            valueOrientation,
            responseCadence
        };

        if (lastInteraction is not null)
        {
            summary.Add($"Last spoke on {lastInteraction.InteractionDate:d} ({lastInteraction.InteractionType}).");
        }

        if (lastOrder is not null)
        {
            summary.Add($"Last order {lastOrder.OrderDate:d} for {lastOrder.TotalAmount:C0}.");
        }

        if (quotes.Any())
        {
            summary.Add($"{quotes.Count} active quote(s) indicates evaluation mindset.");
        }

        return string.Join(' ', summary);
    }
}
