namespace CustomerSnapshot.Domain.Models;

public class Quote
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public DateTime QuoteDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
}
