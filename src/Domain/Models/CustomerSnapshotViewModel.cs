namespace CustomerSnapshot.Domain.Models;

public class CustomerSnapshotViewModel
{
    public Customer Customer { get; set; } = new();
    public CustomerKpis Kpis { get; set; } = new();
    public QuoteSummary OpenQuotes { get; set; } = new();
    public IssueSummary OpenIssues { get; set; } = new();
    public IReadOnlyCollection<Interaction> RecentInteractions { get; set; } = Array.Empty<Interaction>();
    public PersonalityProfile Personality { get; set; } = new();
    public IReadOnlyCollection<TalkingPoint> TalkingPoints { get; set; } = Array.Empty<TalkingPoint>();
    public NextBestAction NextAction { get; set; } = new();
}

public class QuoteSummary
{
    public int OpenCount { get; set; }
    public decimal TotalOpenValue { get; set; }
    public IReadOnlyCollection<Quote> TopQuotes { get; set; } = Array.Empty<Quote>();
}

public class IssueSummary
{
    public int OpenCount { get; set; }
    public bool HasHighSeverity { get; set; }
    public IReadOnlyCollection<Issue> TopIssues { get; set; } = Array.Empty<Issue>();
}
