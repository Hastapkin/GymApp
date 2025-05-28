using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;

namespace GymApp.Data;

public class DbContext
{
    private readonly string _connectionString;

    public DbContext()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuration = builder.Build();
        _connectionString = configuration.GetConnectionString("OracleConnection") ??
            throw new InvalidOperationException("Connection string 'OracleConnection' not found.");
    }

    public OracleConnection GetConnection()
    {
        return new OracleConnection(_connectionString);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }

    public bool TestConnection()
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }
}