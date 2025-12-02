using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public class OrderRepository : SqlRepositoryBase, IOrderRepository
{
    public OrderRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(int customerId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT Id, CustomerId, OrderDate, TotalAmount, MarginAmount
                             FROM vwCustomerOrders
                             WHERE CustomerId = @CustomerId
                               AND (@From IS NULL OR OrderDate >= @From)
                               AND (@To IS NULL OR OrderDate <= @To)
                             ORDER BY OrderDate DESC";

        var results = new List<Order>();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@From", (object?)from ?? DBNull.Value);
        command.Parameters.AddWithValue("@To", (object?)to ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new Order
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                OrderDate = reader.GetDateTime(2),
                TotalAmount = reader.GetDecimal(3),
                MarginAmount = reader.GetDecimal(4)
            });
        }

        return results;
    }
}
