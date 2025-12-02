namespace CustomerSnapshot.Domain.Models;

public class TalkingPoint
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

public class NextBestAction
{
    public string Label { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
