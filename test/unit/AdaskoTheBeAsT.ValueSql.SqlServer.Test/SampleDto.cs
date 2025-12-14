using System;
using AdaskoTheBeAsT.ValueSql.Attributes;

namespace AdaskoTheBeAsT.ValueSql.SqlServer.Test;

[ValueSqlMapper]
public sealed class SampleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [ValueSqlColumn("Description")]
    public string? Desc { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? ExternalId { get; set; }
}
