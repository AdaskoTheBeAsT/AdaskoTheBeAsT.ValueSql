using System.Collections.Generic;
using System.Text;
using AdaskoTheBeAsT.ValueSql.SourceGenerator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AdaskoTheBeAsT.ValueSql.SourceGenerator;

public abstract class SourceGeneratorHelperBase
    : ISourceGeneratorHelper
{
    private readonly IMapperClassGenerator _mapperClassGenerator;

    protected SourceGeneratorHelperBase(
        IMapperClassGenerator mapperClassGenerator)
    {
        _mapperClassGenerator = mapperClassGenerator;
    }

    public void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ValueSqlGeneratorOptions options,
        IList<(INamedTypeSymbol TypeSymbol, IList<PropertyColumnInfo> Properties)> typesToGenerate)
    {
        foreach (var (typeSymbol, properties) in typesToGenerate)
        {
            var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            var className = typeSymbol.Name;

            var content = _mapperClassGenerator.Generate(namespaceName, className, properties);
            context.AddSource($"{className}Mapper.g.cs", SourceText.From(content, Encoding.UTF8));
        }

        GenerateAdditionalFiles(context, options);
    }

    protected abstract void GenerateAdditionalFiles(
        SourceProductionContext context,
        ValueSqlGeneratorOptions options);
}
