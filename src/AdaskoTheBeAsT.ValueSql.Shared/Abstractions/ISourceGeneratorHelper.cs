using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace AdaskoTheBeAsT.ValueSql.SourceGenerator.Abstractions;

public interface ISourceGeneratorHelper
{
    void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ValueSqlGeneratorOptions options,
        IList<(INamedTypeSymbol TypeSymbol, IList<PropertyColumnInfo> Properties)> typesToGenerate);
}
