using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AdaskoTheBeAsT.ValueSql.PerformanceTest.Infrastructure;

public sealed class DatabaseSetup : IAsyncDisposable
{
    private const int RecordCount = 100_000;
    private readonly MsSqlContainer _container;

    public DatabaseSetup()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync().ConfigureAwait(false);
        ConnectionString = _container.GetConnectionString();

        await CreateSchemaAsync().ConfigureAwait(false);
        await SeedDataAsync().ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        return _container.DisposeAsync();
    }

    private async Task CreateSchemaAsync()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context = new ProductDbContext(options);
        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    private async Task SeedDataAsync()
    {
        Console.WriteLine($"Seeding {RecordCount:N0} records...");

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var categories = new[] { "Electronics", "Clothing", "Food", "Books", "Sports", "Home", "Garden", "Toys", "Health", "Beauty" };
        var random = new Random(42);
        var batchSize = 1000;

        for (var batch = 0; batch < RecordCount / batchSize; batch++)
        {
            var transaction = (SqlTransaction)await connection.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                var sql = new System.Text.StringBuilder();
                sql.AppendLine("INSERT INTO Products (Name, Description, Price, Quantity, Category, IsActive, CreatedAt, ModifiedAt, ExternalId) VALUES");

                for (var i = 0; i < batchSize; i++)
                {
                    var recordNum = (batch * batchSize) + i;
                    var category = categories[random.Next(categories.Length)];
                    var hasDescription = random.Next(10) > 2;
                    var hasModifiedAt = random.Next(10) > 5;

                    if (i > 0)
                    {
                        sql.Append(',');
                    }

                    var desc = hasDescription ? $"'Description for product {recordNum}'" : "NULL";
                    var price = Math.Round((decimal)(random.NextDouble() * 1000), 2).ToString(CultureInfo.InvariantCulture);
                    var qty = random.Next(0, 10000);
                    var active = random.Next(10) > 1 ? 1 : 0;
                    var createdAt = DateTime.UtcNow.AddDays(-random.Next(0, 365)).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    var modAt = hasModifiedAt ? $"'{DateTime.UtcNow.AddDays(-random.Next(0, 30)).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}'" : "NULL";
                    var extId = Guid.NewGuid();

                    sql.AppendLine(CultureInfo.InvariantCulture, $"('Product {recordNum}', {desc}, {price}, {qty}, '{category}', {active}, '{createdAt}', {modAt}, '{extId}')");
                }

                command.CommandText = sql.ToString();
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            finally
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }

            if ((batch + 1) % 10 == 0)
            {
                Console.WriteLine($"  Inserted {(batch + 1) * batchSize:N0} records...");
            }
        }

        Console.WriteLine($"Seeding complete. Total records: {RecordCount:N0}");
    }
}
