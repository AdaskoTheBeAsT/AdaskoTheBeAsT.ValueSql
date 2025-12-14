using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Marks a partial interface or class as a ValueSql repository.
/// The source generator will add implementation for methods marked with [ValueSqlMethod].
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public sealed class ValueSqlRepositoryAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the connection property or field name to use.
    /// Default is "_connection" for classes, not applicable for interfaces.
    /// </summary>
    public string ConnectionMember { get; set; } = "_connection";

    /// <summary>
    /// Gets or sets which operations to generate.
    /// Default is All operations.
    /// </summary>
    public ValueSqlOperations Operations { get; set; } = ValueSqlOperations.All;

    /// <summary>
    /// Gets or sets a value indicating whether to generate async methods.
    /// Default is true.
    /// </summary>
    public bool GenerateAsync { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate sync methods.
    /// Default is true.
    /// </summary>
    public bool GenerateSync { get; set; } = true;
}
