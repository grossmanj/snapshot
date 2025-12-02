using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public class IssueRepository : SqlRepositoryBase, IIssueRepository
{
    public IssueRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IReadOnlyList<Issue>> GetOpenIssuesAsync(int customerId, int top = 3, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT TOP(@Top) Id, CustomerId, Severity, Status, Summary, CreatedOn
                             FROM vwCustomerIssues WHERE CustomerId = @CustomerId AND Status <> 'Closed'
                             ORDER BY CreatedOn DESC";

        var results = new List<Issue>();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Top", top);
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new Issue
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                Severity = reader.GetString(2),
                Status = reader.GetString(3),
                Summary = reader.GetString(4),
                CreatedOn = reader.GetDateTime(5)
            });
        }

        return results;
    }
}
