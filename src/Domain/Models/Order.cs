namespace CustomerSnapshot.Domain.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal MarginAmount { get; set; }
}

public class Invoice
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsOverdue => !PaidDate.HasValue && DateTime.UtcNow > DueDate;
    public int DaysToPay => PaidDate.HasValue ? (int)(PaidDate.Value - InvoiceDate).TotalDays : 0;
}
