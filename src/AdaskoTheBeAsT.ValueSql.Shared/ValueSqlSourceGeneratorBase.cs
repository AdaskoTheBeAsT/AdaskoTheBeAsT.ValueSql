using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using AdaskoTheBeAsT.ValueSql.SourceGenerator.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AdaskoTheBeAsT.ValueSql.SourceGenerator;

public abstract class ValueSqlSourceGeneratorBase
{
    private const string ValueSqlMapperAttributeName = "ValueSqlMapper";

    private readonly ISourceGeneratorHelper _sourceGeneratorHelper;

    protected ValueSqlSourceGeneratorBase(
        ISourceGeneratorHelper sourceGeneratorHelper)
    {
        _sourceGeneratorHelper = sourceGeneratorHelper;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var optionsProvider = context.AnalyzerConfigOptionsProvider.Select(SelectOptions);

        var classDeclarations =
            context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null)
                .Select(static (c, _) => c!);

        var compilationAndClasses =
            context.CompilationProvider.Combine(optionsProvider).Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndClasses,
            (spc, source) => Execute(spc, source.Left.Left, source.Left.Right, source.Right));
    }

    private static ValueSqlGeneratorOptions SelectOptions(
        AnalyzerConfigOptionsProvider provider,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var schema = string.Empty;
        if (provider.GlobalOptions.TryGetValue(
                "build_property.AdaskoTheBeAsT_ValueSql_DbSchema",
                out var schemaProperty))
        {
            schema = schemaProperty;
        }

        var useOrdinal = false;
        if (provider.GlobalOptions.TryGetValue(
                "build_property.AdaskoTheBeAsT_ValueSql_UseOrdinal",
                out var useOrdinalValue) && bool.TryParse(useOrdinalValue, out var result))
        {
            useOrdinal = result;
        }

        return new ValueSqlGeneratorOptions(schema, useOrdinal);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is not TypeDeclarationSyntax typeDeclaration)
        {
            return false;
        }

        return typeDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString().Contains(ValueSqlMapperAttributeName, StringComparison.Ordinal));
    }

    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context) =>
        context.Node as TypeDeclarationSyntax;

    private static IList<(INamedTypeSymbol TypeSymbol, IList<PropertyColumnInfo> Properties)> GetTypesToGenerate(
        Compilation compilation,
        IEnumerable<TypeDeclarationSyntax>? distinctTypeDeclarations,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var result = new List<(INamedTypeSymbol, IList<PropertyColumnInfo>)>();

        if (distinctTypeDeclarations == null)
        {
            return result;
        }

        var mapperAttributeSymbol = compilation.GetTypeByMetadataName(
            "AdaskoTheBeAsT.ValueSql.Attributes.ValueSqlMapperAttribute");
        var columnAttributeSymbol = compilation.GetTypeByMetadataName(
            "AdaskoTheBeAsT.ValueSql.Attributes.ValueSqlColumnAttribute");
        var ignoreAttributeSymbol = compilation.GetTypeByMetadataName(
            "AdaskoTheBeAsT.ValueSql.Attributes.ValueSqlIgnoreAttribute");

        foreach (var typeDeclarationSyntax in distinctTypeDeclarations)
        {
            token.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol)
            {
                continue;
            }

            var hasMapperAttribute = typeSymbol.GetAttributes()
                .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mapperAttributeSymbol));

            if (!hasMapperAttribute)
            {
                continue;
            }

            var properties = ExtractProperties(typeSymbol, columnAttributeSymbol, ignoreAttributeSymbol);
            result.Add((typeSymbol, properties));
        }

        return result;
    }

    private static IList<PropertyColumnInfo> ExtractProperties(
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol? columnAttributeSymbol,
        INamedTypeSymbol? ignoreAttributeSymbol)
    {
        var properties = new List<PropertyColumnInfo>();

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            var propertyInfo = TryExtractPropertyInfo(propertySymbol, columnAttributeSymbol, ignoreAttributeSymbol);
            if (propertyInfo != null)
            {
                properties.Add(propertyInfo);
            }
        }

        return properties;
    }

    private static PropertyColumnInfo? TryExtractPropertyInfo(
        IPropertySymbol propertySymbol,
        INamedTypeSymbol? columnAttributeSymbol,
        INamedTypeSymbol? ignoreAttributeSymbol)
    {
        if (propertySymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return null;
        }

        if (propertySymbol.SetMethod == null)
        {
            return null;
        }

        var hasIgnoreAttribute = ignoreAttributeSymbol != null &&
            propertySymbol.GetAttributes()
                .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, ignoreAttributeSymbol));

        if (hasIgnoreAttribute)
        {
            return null;
        }

        var (columnName, ordinal) = ExtractColumnInfo(propertySymbol, columnAttributeSymbol);

        var isNullable = propertySymbol.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                         propertySymbol.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

        var propertyType = propertySymbol.Type.ToDisplayString();

        return new PropertyColumnInfo(
            propertySymbol.Name,
            propertyType,
            columnName,
            ordinal,
            isNullable);
    }

    private static (string ColumnName, int? Ordinal) ExtractColumnInfo(
        IPropertySymbol propertySymbol,
        INamedTypeSymbol? columnAttributeSymbol)
    {
        var columnName = propertySymbol.Name;
        int? ordinal = null;

        var columnAttribute = columnAttributeSymbol != null
            ? propertySymbol.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, columnAttributeSymbol))
            : null;

        if (columnAttribute != null)
        {
            foreach (var arg in columnAttribute.ConstructorArguments)
            {
                if (arg.Type?.SpecialType == SpecialType.System_String && arg.Value is string name)
                {
                    columnName = name;
                }
                else if (arg.Type?.SpecialType == SpecialType.System_Int32 && arg.Value is int ord)
                {
                    ordinal = ord;
                }
            }
        }

        return (columnName, ordinal);
    }

    private void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ValueSqlGeneratorOptions options,
        ImmutableArray<TypeDeclarationSyntax> typeDeclarations)
    {
        if (typeDeclarations.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctTypeDeclarations = typeDeclarations.Distinct();
        var typesToGenerate = GetTypesToGenerate(
            compilation,
            distinctTypeDeclarations,
            context.CancellationToken);

        if (typesToGenerate.Count > 0)
        {
            _sourceGeneratorHelper.GenerateCode(context, compilation, options, typesToGenerate);
        }
    }
}
