namespace AdaskoTheBeAsT.ValueSql.SourceGenerator;

public sealed class ValueSqlGeneratorOptions
{
    public ValueSqlGeneratorOptions(
        string schema,
        bool useOrdinal)
    {
        Schema = schema;
        UseOrdinal = useOrdinal;
    }

    public string Schema { get; }

    public bool UseOrdinal { get; }
}
