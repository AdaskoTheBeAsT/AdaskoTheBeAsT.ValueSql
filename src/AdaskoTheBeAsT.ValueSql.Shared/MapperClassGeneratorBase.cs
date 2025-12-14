using System.Collections.Generic;
using System.Text;
using AdaskoTheBeAsT.ValueSql.SourceGenerator.Abstractions;

namespace AdaskoTheBeAsT.ValueSql.SourceGenerator;

public abstract class MapperClassGeneratorBase
    : IMapperClassGenerator
{
    public string Generate(
        string namespaceName,
        string className,
        IList<PropertyColumnInfo> properties)
    {
        var sb = new StringBuilder();
        GenerateUsings(sb);
        GenerateNamespaceStart(sb, namespaceName);
        GenerateClassStart(sb, className);
        GenerateMapMethod(sb, className, properties);
        GenerateClassEnd(sb);
        GenerateNamespaceEnd(sb);
        return sb.ToString();
    }

    protected virtual void GenerateUsings(StringBuilder sb)
    {
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Data;");
        sb.AppendLine("using AdaskoTheBeAsT.ValueSql.Abstractions;");
        sb.AppendLine();
    }

    protected virtual void GenerateNamespaceStart(StringBuilder sb, string namespaceName)
    {
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
    }

    protected virtual void GenerateClassStart(StringBuilder sb, string className)
    {
        sb.AppendLine($"public sealed class {className}Mapper : IValueSqlMapper<{className}>");
        sb.AppendLine("{");
    }

    protected abstract void GenerateMapMethod(
        StringBuilder sb,
        string className,
        IList<PropertyColumnInfo> properties);

    protected virtual void GenerateClassEnd(StringBuilder sb)
    {
        sb.AppendLine("}");
    }

    protected virtual void GenerateNamespaceEnd(StringBuilder sb)
    {
    }

    protected abstract string GetReaderMethod(string propertyType, bool isNullable);
}
