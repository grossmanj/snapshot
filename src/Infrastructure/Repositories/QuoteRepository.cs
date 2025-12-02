using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public class QuoteRepository : SqlRepositoryBase, IQuoteRepository
{
    public QuoteRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IReadOnlyList<Quote>> GetOpenQuotesAsync(int customerId, int top = 3, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT TOP(@Top) Id, CustomerId, QuoteNumber, QuoteDate, Amount, Description, IsOpen
                             FROM vwCustomerQuotes
                             WHERE CustomerId = @CustomerId AND IsOpen = 1
                             ORDER BY QuoteDate DESC";

        var results = new List<Quote>();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Top", top);
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new Quote
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                QuoteNumber = reader.GetString(2),
                QuoteDate = reader.GetDateTime(3),
                Amount = reader.GetDecimal(4),
                Description = reader.GetString(5),
                IsOpen = reader.GetBoolean(6)
            });
        }

        return results;
    }
}
