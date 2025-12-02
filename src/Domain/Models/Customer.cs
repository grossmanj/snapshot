namespace CustomerSnapshot.Domain.Models;

public class Customer
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Segment { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public DateTime LastRefreshedUtc { get; set; }
}
