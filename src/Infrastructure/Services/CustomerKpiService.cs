using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace CustomerSnapshot.Infrastructure.Services;

public class CustomerKpiService : SqlRepositoryBase, ICustomerKpiService
{
    public CustomerKpiService(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<CustomerKpis> GetCustomerKpisAsync(int customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT
    SUM(CASE WHEN YEAR(OrderDate) = YEAR(GETDATE()) THEN TotalAmount ELSE 0 END) AS TotalSpendYtd,
    SUM(CASE WHEN YEAR(OrderDate) = YEAR(GETDATE()) - 1 THEN TotalAmount ELSE 0 END) AS TotalSpendLy,
    AVG(CASE WHEN YEAR(OrderDate) = YEAR(GETDATE()) THEN TotalAmount END) AS AvgOrderYtd,
    AVG(CASE WHEN YEAR(OrderDate) = YEAR(GETDATE()) - 1 THEN TotalAmount END) AS AvgOrderLy
FROM vwCustomerOrders WHERE CustomerId = @CustomerId;

SELECT TOP 3 DATEFROMPARTS(YEAR(OrderDate), MONTH(OrderDate), 1) AS PeriodStart,
       SUM(MarginAmount) AS MarginAmount,
       SUM(TotalAmount) AS Revenue
FROM vwCustomerOrders
WHERE CustomerId = @CustomerId
GROUP BY DATEFROMPARTS(YEAR(OrderDate), MONTH(OrderDate), 1)
ORDER BY PeriodStart DESC;

SELECT InvoiceDate, DueDate, PaidDate, Amount FROM vwCustomerInvoices WHERE CustomerId = @CustomerId;
";

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);

        var kpis = new CustomerKpis();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            kpis.TotalSpendYtd = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
            kpis.TotalSpendLastYear = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
            kpis.AverageOrderSizeYtd = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
            kpis.AverageOrderSizeLastYear = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
            kpis.PercentDeltaVsLy = CalculateDelta(kpis.TotalSpendYtd, kpis.TotalSpendLastYear);
        }

        var marginPoints = new List<(DateTime PeriodStart, decimal MarginPct)>();
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var period = reader.GetDateTime(0);
                var marginAmount = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                var revenue = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                var pct = revenue == 0 ? 0 : marginAmount / revenue;
                marginPoints.Add((period, pct));
            }
        }

        var invoices = new List<Invoice>();
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                invoices.Add(new Invoice
                {
                    InvoiceDate = reader.GetDateTime(0),
                    DueDate = reader.GetDateTime(1),
                    PaidDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    Amount = reader.GetDecimal(3)
                });
            }
        }

        kpis.MarginTrend = EvaluateMarginTrend(marginPoints);
        (kpis.AverageDaysToPay, kpis.PercentInvoicesLate, kpis.PaymentBehavior) = EvaluatePaymentBehavior(invoices);
        kpis.RiskRating = CalculateRiskRating(kpis);

        return kpis;
    }

    private static decimal CalculateDelta(decimal currentValue, decimal previousValue)
    {
        if (previousValue == 0)
        {
            return currentValue > 0 ? 100 : 0;
        }

        return Math.Round((currentValue - previousValue) / previousValue * 100, 1);
    }

    private static string EvaluateMarginTrend(IReadOnlyList<(DateTime PeriodStart, decimal MarginPct)> marginPoints)
    {
        if (marginPoints.Count < 2)
        {
            return "Stable";
        }

        var recent = marginPoints.OrderByDescending(p => p.PeriodStart).Take(3).ToArray();
        var deltas = new List<decimal>();
        for (var i = 0; i < recent.Length - 1; i++)
        {
            deltas.Add(recent[i].MarginPct - recent[i + 1].MarginPct);
        }

        var averageChange = deltas.Average();
        if (averageChange > 0.01m)
        {
            return "Improving";
        }

        if (averageChange < -0.01m)
        {
            return "Declining";
        }

        return "Stable";
    }

    private static (double AverageDaysToPay, double PercentLate, string Behavior) EvaluatePaymentBehavior(IEnumerable<Invoice> invoices)
    {
        var paidInvoices = invoices.Where(i => i.PaidDate.HasValue).ToList();
        var daysToPay = paidInvoices.Select(i => (i.PaidDate!.Value - i.InvoiceDate).TotalDays).ToList();
        var averageDays = daysToPay.Any() ? daysToPay.Average() : 0;

        var lateInvoices = invoices.Count(i => (i.PaidDate ?? DateTime.UtcNow) > i.DueDate);
        var totalInvoices = invoices.Count();
        var percentLate = totalInvoices == 0 ? 0 : (double)lateInvoices / totalInvoices * 100;

        var behavior = averageDays == 0 && percentLate == 0
            ? "No payment history"
            : averageDays <= 30 && percentLate < 10 ? "Pays on time"
            : percentLate > 40 ? "Frequently late"
            : "Occasional delays";

        return (Math.Round(averageDays, 1), Math.Round(percentLate, 1), behavior);
    }

    private static string CalculateRiskRating(CustomerKpis kpis)
    {
        var riskScore = 0;

        if (kpis.PercentDeltaVsLy < -10)
        {
            riskScore += 2;
        }

        if (kpis.PercentInvoicesLate > 30)
        {
            riskScore += 2;
        }
        else if (kpis.PercentInvoicesLate > 10)
        {
            riskScore += 1;
        }

        if (kpis.MarginTrend.Equals("Declining", StringComparison.OrdinalIgnoreCase))
        {
            riskScore += 1;
        }

        return riskScore switch
        {
            >= 4 => "Risky",
            2 or 3 => "Watch",
            _ => "Good"
        };
    }
}
