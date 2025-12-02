namespace CustomerSnapshot.Domain.Models;

public class Issue
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Severity { get; set; } = string.Empty; // e.g., Low, Medium, High
    public string Status { get; set; } = string.Empty;   // e.g., Open, Closed, In Progress
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
