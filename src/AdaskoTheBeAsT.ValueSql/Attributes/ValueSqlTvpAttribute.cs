using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Marks a class or struct for Table-Valued Parameter (TVP) code generation.
/// Generates optimized mapping code to convert collections to SQL Server TVPs.
/// </summary>
/// <example>
/// <code>
/// [ValueSqlTvp("dbo.ProductTableType")]
/// public class ProductTvp
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
///
/// // Usage:
/// var tvpMapper = default(ProductTvpMapper);
/// var parameter = new SqlParameter("@products", SqlDbType.Structured)
/// {
///     TypeName = "dbo.ProductTableType",
///     Value = tvpMapper.ToSqlDataRecords(products)
/// };
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ValueSqlTvpAttribute : Attribute
{
    /// <summary>
    /// Creates a new TVP attribute with the specified SQL type name.
    /// </summary>
    /// <param name="typeName">The SQL Server user-defined table type name (e.g., "dbo.ProductTableType").</param>
    public ValueSqlTvpAttribute(string typeName)
    {
        TypeName = typeName;
    }

    /// <summary>
    /// The SQL Server user-defined table type name.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Optional schema override. If not set, schema is extracted from TypeName.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Specifies which TVP mapping implementations to generate.
    /// Default is SqlDataRecord (fastest).
    /// </summary>
    public TvpGenerationMode GenerationMode { get; set; } = TvpGenerationMode.SqlDataRecord;

    /// <summary>
    /// Gets or sets a value indicating whether to generate SQL migration script.
    /// When true, generates a .sql file in the Scripts folder with CREATE TYPE statement.
    /// Default is false.
    /// </summary>
    public bool GenerateMigrationScript { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate a validation class.
    /// When true, generates a validator to compare DB schema with C# definition.
    /// Default is false.
    /// </summary>
    public bool GenerateValidator { get; set; }

    /// <summary>
    /// Gets or sets the output folder for generated SQL scripts.
    /// Relative to project root. Default is "Scripts/Tvp".
    /// </summary>
    public string ScriptFolder { get; set; } = "Scripts/Tvp";
}
