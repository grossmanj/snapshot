using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public class InvoiceRepository : SqlRepositoryBase, IInvoiceRepository
{
    public InvoiceRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IReadOnlyList<Invoice>> GetInvoicesAsync(int customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT Id, CustomerId, InvoiceNumber, InvoiceDate, DueDate, Amount, PaidDate
                             FROM vwCustomerInvoices WHERE CustomerId = @CustomerId";

        var results = new List<Invoice>();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new Invoice
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                InvoiceNumber = reader.GetString(2),
                InvoiceDate = reader.GetDateTime(3),
                DueDate = reader.GetDateTime(4),
                Amount = reader.GetDecimal(5),
                PaidDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
            });
        }

        return results;
    }
}
