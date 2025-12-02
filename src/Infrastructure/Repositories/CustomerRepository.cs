using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public class CustomerRepository : SqlRepositoryBase, ICustomerRepository
{
    public CustomerRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT TOP 1 Id, CustomerCode, Name, Segment, Industry, LastRefreshedUtc
                             FROM vwCustomers WHERE Id = @CustomerId";

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Customer
        {
            Id = reader.GetInt32(0),
            Code = reader.GetString(1),
            Name = reader.GetString(2),
            Segment = reader.GetString(3),
            Industry = reader.GetString(4),
            LastRefreshedUtc = reader.IsDBNull(5) ? DateTime.UtcNow : reader.GetDateTime(5)
        };
    }
}
