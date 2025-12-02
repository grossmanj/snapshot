namespace CustomerSnapshot.Domain.Models;

public class Interaction
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime InteractionDate { get; set; }
    public string InteractionType { get; set; } = string.Empty; // Call, Email, Meeting, Ticket, Quote
    public string Subject { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}
