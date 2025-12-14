using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdaskoTheBeAsT.ValueSql.PerformanceTest.Entities;
using AdaskoTheBeAsT.ValueSql.PerformanceTest.Infrastructure;
using AdaskoTheBeAsT.ValueSql.Runtime;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AdaskoTheBeAsT.ValueSql.PerformanceTest.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class QuickBenchmarks
{
    private const string Query1 = "SELECT TOP 1 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";
    private const string Query10 = "SELECT TOP 10 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";
    private const string Query100 = "SELECT TOP 100 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";
    private const string Query1K = "SELECT TOP 1000 Id, Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId FROM Products";

    private static DatabaseSetup? _sharedSetup;
    private string _connectionString = null!;
    private DbContextOptions<ProductDbContext> _dbContextOptions = null!;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        if (_sharedSetup == null)
        {
            Console.WriteLine("Starting SQL Server container and seeding data...");
            _sharedSetup = new DatabaseSetup();
            await _sharedSetup.InitializeAsync().ConfigureAwait(false);
            Console.WriteLine("Setup complete!");
        }

        _connectionString = _sharedSetup.ConnectionString;
        _dbContextOptions = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
    }

    [Benchmark(Description = "ValueSql Buffered - 1 row")]
    public async Task<List<Product>> ValueSqlBuffered_1()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query1;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult).ConfigureAwait(false);
        return await ValueSqlBulkReader.ReadAllBufferedAsync<Product, ProductMapper>(reader, default, 1).ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Fast - 1 row")]
    public async Task<List<Product>> ValueSqlFast_1()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query1;

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult).ConfigureAwait(false);
        return await ValueSqlReader.ReadAllAsync<Product, ProductMapper>(reader, default, 1).ConfigureAwait(false);
    }

    [Benchmark(Description = "Dapper - 1 row")]
    public async Task<List<Product>> Dapper_1()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(Query1).ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - 1 row")]
    public async Task<List<Product>> EfCore_1()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Take(1)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Buffered - 10 rows")]
    public async Task<List<Product>> ValueSqlBuffered_10()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query10;

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        return await ValueSqlBulkReader.ReadAllBufferedAsync<Product, ProductMapper>(reader, default, 10).ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Fast - 10 rows")]
    public async Task<List<Product>> ValueSqlFast_10()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query10;

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        return await ValueSqlReader.ReadAllAsync<Product, ProductMapper>(reader, default, 10).ConfigureAwait(false);
    }

    [Benchmark(Description = "Dapper - 10 rows")]
    public async Task<List<Product>> Dapper_10()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(Query10).ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - 10 rows")]
    public async Task<List<Product>> EfCore_10()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Take(10)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Buffered - 100 rows")]
    public async Task<List<Product>> ValueSqlBuffered_100()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query100;

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        return await ValueSqlBulkReader.ReadAllBufferedAsync<Product, ProductMapper>(reader, default, 100).ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Fast - 100 rows")]
    public async Task<List<Product>> ValueSqlFast_100()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query100;

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        return await ValueSqlReader.ReadAllAsync<Product, ProductMapper>(reader, default, 100).ConfigureAwait(false);
    }

    [Benchmark(Description = "Dapper - 100 rows")]
    public async Task<List<Product>> Dapper_100()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(Query100).ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - 100 rows")]
    public async Task<List<Product>> EfCore_100()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Take(100)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Buffered - 1K rows")]
    public async Task<List<Product>> ValueSqlBuffered_1K()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query1K;

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        return await ValueSqlBulkReader.ReadAllBufferedAsync<Product, ProductMapper>(reader, default, 1000).ConfigureAwait(false);
    }

    [Benchmark(Description = "ValueSql Fast - 1K rows")]
    public async Task<List<Product>> ValueSqlFast_1K()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = Query1K;

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        return await ValueSqlReader.ReadAllAsync<Product, ProductMapper>(reader, default, 1000).ConfigureAwait(false);
    }

    [Benchmark(Description = "Dapper - 1K rows")]
    public async Task<List<Product>> Dapper_1K()
    {
        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Product>(Query1K).ConfigureAwait(false);
        return result.ToList();
    }

    [Benchmark(Description = "EF Core - 1K rows")]
    public async Task<List<Product>> EfCore_1K()
    {
        await using var context = new ProductDbContext(_dbContextOptions);
        return await context.Products
            .AsNoTracking()
            .Take(1000)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    // Sync variants - often faster for small datasets
    [Benchmark(Description = "ValueSql Sync - 10 rows")]
    public List<Product> ValueSqlSync_10()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = Query10;

        using var reader = command.ExecuteReader();
        return ValueSqlReader.ReadAllSync<Product, ProductMapper>(reader, default, 10);
    }

    [Benchmark(Description = "ValueSql Sync - 100 rows")]
    public List<Product> ValueSqlSync_100()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = Query100;

        using var reader = command.ExecuteReader();
        return ValueSqlReader.ReadAllSync<Product, ProductMapper>(reader, default, 100);
    }

    [Benchmark(Description = "ValueSql Sync - 1K rows")]
    public List<Product> ValueSqlSync_1K()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = Query1K;

        using var reader = command.ExecuteReader();
        return ValueSqlReader.ReadAllSync<Product, ProductMapper>(reader, default, 1000);
    }
}
