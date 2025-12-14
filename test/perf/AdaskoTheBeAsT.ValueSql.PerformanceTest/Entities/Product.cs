using System;
using AdaskoTheBeAsT.ValueSql.Attributes;

namespace AdaskoTheBeAsT.ValueSql.PerformanceTest.Entities;

[ValueSqlMapper]
public sealed class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid ExternalId { get; set; }
}
