using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using System.Linq;

namespace CustomerSnapshot.Application.Services;

public class CustomerSnapshotService : ICustomerSnapshotService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerKpiService _customerKpiService;
    private readonly IQuoteRepository _quoteRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly IInteractionRepository _interactionRepository;
    private readonly IPersonalityProfileService _personalityProfileService;
    private readonly ITalkingPointsService _talkingPointsService;
    private readonly INextBestActionService _nextBestActionService;

    public CustomerSnapshotService(
        ICustomerRepository customerRepository,
        ICustomerKpiService customerKpiService,
        IQuoteRepository quoteRepository,
        IIssueRepository issueRepository,
        IInteractionRepository interactionRepository,
        IPersonalityProfileService personalityProfileService,
        ITalkingPointsService talkingPointsService,
        INextBestActionService nextBestActionService)
    {
        _customerRepository = customerRepository;
        _customerKpiService = customerKpiService;
        _quoteRepository = quoteRepository;
        _issueRepository = issueRepository;
        _interactionRepository = interactionRepository;
        _personalityProfileService = personalityProfileService;
        _talkingPointsService = talkingPointsService;
        _nextBestActionService = nextBestActionService;
    }

    public async Task<CustomerSnapshotViewModel?> GetSnapshotAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetCustomerAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        var kpisTask = _customerKpiService.GetCustomerKpisAsync(customerId, cancellationToken);
        var quotesTask = _quoteRepository.GetOpenQuotesAsync(customerId, 3, cancellationToken);
        var issuesTask = _issueRepository.GetOpenIssuesAsync(customerId, 3, cancellationToken);
        var interactionsTask = _interactionRepository.GetRecentInteractionsAsync(customerId, 5, cancellationToken);

        await Task.WhenAll(kpisTask, quotesTask, issuesTask, interactionsTask);

        var profile = await _personalityProfileService.BuildProfileAsync(customerId, cancellationToken);
        var talkingPoints = await _talkingPointsService.BuildTalkingPointsAsync(customerId, profile, cancellationToken);
        var nextBestAction = await _nextBestActionService.DetermineNextActionAsync(customerId, cancellationToken);

        var quotes = quotesTask.Result;
        var issues = issuesTask.Result;
        var interactions = interactionsTask.Result;

        return new CustomerSnapshotViewModel
        {
            Customer = customer,
            Kpis = kpisTask.Result,
            OpenQuotes = new QuoteSummary
            {
                OpenCount = quotes.Count,
                TotalOpenValue = quotes.Sum(q => q.Amount),
                TopQuotes = quotes
            },
            OpenIssues = new IssueSummary
            {
                OpenCount = issues.Count,
                HasHighSeverity = issues.Any(i => i.Severity.Equals("High", StringComparison.OrdinalIgnoreCase)),
                TopIssues = issues
            },
            RecentInteractions = interactions,
            Personality = profile,
            TalkingPoints = talkingPoints,
            NextAction = nextBestAction
        };
    }
}
