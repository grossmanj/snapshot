namespace CustomerSnapshot.Domain.Models;

public class CustomerKpis
{
    public decimal TotalSpendYtd { get; set; }
    public decimal TotalSpendLastYear { get; set; }
    public decimal PercentDeltaVsLy { get; set; }
    public decimal AverageOrderSizeYtd { get; set; }
    public decimal AverageOrderSizeLastYear { get; set; }
    public string MarginTrend { get; set; } = string.Empty;
    public string PaymentBehavior { get; set; } = string.Empty;
    public double AverageDaysToPay { get; set; }
    public double PercentInvoicesLate { get; set; }
    public string RiskRating { get; set; } = string.Empty;
}
