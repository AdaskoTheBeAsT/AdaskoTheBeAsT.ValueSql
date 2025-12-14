using Microsoft.CodeAnalysis;

namespace AdaskoTheBeAsT.ValueSql.SqlServer;

[Generator]
public sealed class SrcGen
    : IIncrementalGenerator
{
    private readonly SqlServerValueSqlSourceGenerator _generator = new SqlServerValueSqlSourceGenerator();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _generator.Initialize(context);
    }
}
