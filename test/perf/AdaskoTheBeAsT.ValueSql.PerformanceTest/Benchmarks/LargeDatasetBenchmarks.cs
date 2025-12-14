using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.ValueSql.PerformanceTest.Entities;
using AdaskoTheBeAsT.ValueSql.PerformanceTest.Infrastructure;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AdaskoTheBeAsT.ValueSql.PerformanceTest.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class LargeDatasetBenchmarks
{
    private string _connectionString = null!;
    private DbContextOptions<ProductDbContext> _dbContextOptions = null!;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        var setup = new DatabaseSetup();
        await setup.InitializeAsync().ConfigureAwait(false);
        _connectionString = setup.ConnectionString;

        _dbContextOptions = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
    }

    [Benchmark(Description = "ValueSql - Select 10K rows")]
    public async Task<List<Product>> ValueSql_Select10KAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var mapper = new ProductMapper();
        var products = new List<Product>(10000);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP 10000 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            products.Add(mapper.Map(reader));
        }

        return products;
    }

    [Benchmark(Description = "Dapper - Select 10K rows")]
    public async Task<List<Product>> Dapper_Select10KAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(
            "SELECT TOP 10000 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products").ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - Select 10K rows")]
    public async Task<List<Product>> EfCore_Select10KAsync()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Take(10000)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql - Select All 100K rows")]
    public async Task<List<Product>> ValueSql_SelectAllAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var mapper = new ProductMapper();
        var products = new List<Product>(100000);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            products.Add(mapper.Map(reader));
        }

        return products;
    }

    [Benchmark(Description = "Dapper - Select All 100K rows")]
    public async Task<List<Product>> Dapper_SelectAllAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(
            "SELECT Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products").ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - Select All 100K rows")]
    public async Task<List<Product>> EfCore_SelectAllAsync()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
