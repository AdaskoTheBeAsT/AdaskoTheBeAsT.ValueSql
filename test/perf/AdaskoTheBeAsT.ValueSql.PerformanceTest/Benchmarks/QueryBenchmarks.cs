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
public class QueryBenchmarks
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

    [Benchmark(Description = "ValueSql - Select All (1000 rows)")]
    public async Task<List<Product>> ValueSql_SelectTop1000Async()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var mapper = new ProductMapper();
        var products = new List<Product>(1000);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP 1000 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            products.Add(mapper.Map(reader));
        }

        return products;
    }

    [Benchmark(Description = "Dapper - Select All (1000 rows)")]
    public async Task<List<Product>> Dapper_SelectTop1000Async()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(
            "SELECT TOP 1000 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products").ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - Select All (1000 rows)")]
    public async Task<List<Product>> EfCore_SelectTop1000Async()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Take(1000)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql - Select by Category")]
    public async Task<List<Product>> ValueSql_SelectByCategoryAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var mapper = new ProductMapper();
        var products = new List<Product>();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products WHERE Category = @Category";
        command.Parameters.AddWithValue("@Category", "Electronics");

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            products.Add(mapper.Map(reader));
        }

        return products;
    }

    [Benchmark(Description = "Dapper - Select by Category")]
    public async Task<List<Product>> Dapper_SelectByCategoryAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(
            "SELECT Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products WHERE Category = @Category",
            new { Category = "Electronics" }).ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - Select by Category")]
    public async Task<List<Product>> EfCore_SelectByCategoryAsync()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Where(p => p.Category == "Electronics")
            .ToListAsync()
            .ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql - Single Row by Id")]
    public async Task<Product?> ValueSql_SingleByIdAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var mapper = new ProductMapper();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP 1 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", 50000);

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        if (await reader.ReadAsync().ConfigureAwait(false))
        {
            return mapper.Map(reader);
        }

        return null;
    }

    [Benchmark(Description = "Dapper - Single Row by Id")]
    public async Task<Product?> Dapper_SingleByIdAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT TOP 1 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products WHERE Id = @Id",
            new { Id = 50000 }).ConfigureAwait(false);
    }

    [Benchmark(Description = "EF Core - Single Row by Id")]
    public async Task<Product?> EfCore_SingleByIdAsync()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == 50000)
            .ConfigureAwait(false);
    }
}
