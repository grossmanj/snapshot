namespace CustomerSnapshot.Domain.Models;

public class PersonalityProfile
{
    public string CommunicationPreference { get; set; } = string.Empty; // Detail or Brevity
    public string ValueOrientation { get; set; } = string.Empty; // Price-sensitive or Value-first
    public string ResponseCadence { get; set; } = string.Empty; // Fast or Slow
    public string Notes { get; set; } = string.Empty;
}
