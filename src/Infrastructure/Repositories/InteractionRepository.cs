using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public class InteractionRepository : SqlRepositoryBase, IInteractionRepository
{
    public InteractionRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IReadOnlyList<Interaction>> GetRecentInteractionsAsync(int customerId, int take, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT TOP(@Take) Id, CustomerId, InteractionDate, InteractionType, Subject, Owner
                             FROM vwCustomerInteractions
                             WHERE CustomerId = @CustomerId
                             ORDER BY InteractionDate DESC";

        var results = new List<Interaction>();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@Take", take);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new Interaction
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                InteractionDate = reader.GetDateTime(2),
                InteractionType = reader.GetString(3),
                Subject = reader.GetString(4),
                Owner = reader.GetString(5)
            });
        }

        return results;
    }
}
