using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace CustomerSnapshot.Infrastructure.Repositories;

public abstract class SqlRepositoryBase
{
    private readonly string _connectionString;

    protected SqlRepositoryBase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ErpDatabase")
            ?? throw new InvalidOperationException("Connection string 'ErpDatabase' is not configured.");
    }

    protected SqlConnection CreateConnection() => new(_connectionString);
}
