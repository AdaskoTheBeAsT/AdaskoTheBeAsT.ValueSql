using System;
using System.Collections.Generic;
using System.Text;
using AdaskoTheBeAsT.ValueSql.SourceGenerator;

namespace AdaskoTheBeAsT.ValueSql.SqlServer;

public sealed class SqlServerMapperClassGenerator
    : MapperClassGeneratorBase
{
#pragma warning disable MA0109 // netstandard2.0 doesn't support collection expressions
    private static readonly Dictionary<string, string> TypeToReaderMethod = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["bool"] = "GetBoolean",
        ["System.Boolean"] = "GetBoolean",
        ["byte"] = "GetByte",
        ["System.Byte"] = "GetByte",
        ["short"] = "GetInt16",
        ["System.Int16"] = "GetInt16",
        ["int"] = "GetInt32",
        ["System.Int32"] = "GetInt32",
        ["long"] = "GetInt64",
        ["System.Int64"] = "GetInt64",
        ["float"] = "GetFloat",
        ["System.Single"] = "GetFloat",
        ["double"] = "GetDouble",
        ["System.Double"] = "GetDouble",
        ["decimal"] = "GetDecimal",
        ["System.Decimal"] = "GetDecimal",
        ["string"] = "GetString",
        ["System.String"] = "GetString",
        ["System.DateTime"] = "GetDateTime",
        ["System.Guid"] = "GetGuid",
        ["char"] = "GetChar",
        ["System.Char"] = "GetChar",
    };

    private static readonly Dictionary<string, string> TypeToFastMethod = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["bool"] = "GetBooleanFast",
        ["System.Boolean"] = "GetBooleanFast",
        ["byte"] = "GetByte",
        ["System.Byte"] = "GetByte",
        ["short"] = "GetInt16",
        ["System.Int16"] = "GetInt16",
        ["int"] = "GetInt32Fast",
        ["System.Int32"] = "GetInt32Fast",
        ["long"] = "GetInt64Fast",
        ["System.Int64"] = "GetInt64Fast",
        ["float"] = "GetFloat",
        ["System.Single"] = "GetFloat",
        ["double"] = "GetDoubleFast",
        ["System.Double"] = "GetDoubleFast",
        ["decimal"] = "GetDecimalFast",
        ["System.Decimal"] = "GetDecimalFast",
        ["string"] = "GetStringFast",
        ["System.String"] = "GetStringFast",
        ["System.DateTime"] = "GetDateTimeFast",
        ["System.Guid"] = "GetGuidFast",
        ["char"] = "GetChar",
        ["System.Char"] = "GetChar",
    };
#pragma warning restore MA0109

    protected override void GenerateUsings(StringBuilder sb)
    {
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Data;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using AdaskoTheBeAsT.ValueSql.Abstractions;");
        sb.AppendLine("using AdaskoTheBeAsT.ValueSql.Runtime;");
        sb.AppendLine("using Microsoft.Data.SqlClient;");
        sb.AppendLine();
    }

    protected override void GenerateClassStart(StringBuilder sb, string className)
    {
        sb.AppendLine($"public readonly struct {className}Mapper : IValueSqlMapper<{className}>, IValueSqlMapperFast<{className}>");
        sb.AppendLine("{");
    }

    protected override void GenerateMapMethod(
        StringBuilder sb,
        string className,
        IList<PropertyColumnInfo> properties)
    {
        GenerateOrdinalCache(sb, properties);
        sb.AppendLine();
        GenerateLegacyMapMethod(sb, className, properties);
        sb.AppendLine();
        GenerateFastMapMethod(sb, className, properties);
        sb.AppendLine();
        GenerateFastMapMethodWithCachedOrdinals(sb, className, properties);
    }

    protected override string GetReaderMethod(string propertyType, bool isNullable)
    {
        var baseType = GetBaseType(propertyType);

        if (TypeToReaderMethod.TryGetValue(baseType, out var method))
        {
            return method;
        }

        return "GetValue";
    }

    private static void GenerateLegacyMapMethod(
        StringBuilder sb,
        string className,
        IList<PropertyColumnInfo> properties)
    {
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine($"    public {className} Map(IDataReader reader)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {className}");
        sb.AppendLine("        {");

        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var comma = i < properties.Count - 1 ? "," : string.Empty;
            var ordinal = prop.Ordinal ?? i;

            if (prop.IsNullable)
            {
                var baseType = GetBaseType(prop.PropertyType);
                var readerMethod = GetReaderMethodInternal(baseType);
                sb.AppendLine($"            {prop.PropertyName} = reader.IsDBNull({ordinal}) ? null : reader.{readerMethod}({ordinal}){comma}");
            }
            else
            {
                var readerMethod = GetReaderMethodInternal(prop.PropertyType);
                sb.AppendLine($"            {prop.PropertyName} = reader.{readerMethod}({ordinal}){comma}");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
    }

    private static void GenerateFastMapMethod(
        StringBuilder sb,
        string className,
        IList<PropertyColumnInfo> properties)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// High-performance mapping using SqlDataReader directly with no virtual dispatch.");
        sb.AppendLine("    /// Uses AggressiveInlining and SkipLocalsInit for maximum throughput.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    [SkipLocalsInit]");
        sb.AppendLine($"    public {className} MapFast(SqlDataReader reader)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {className}");
        sb.AppendLine("        {");

        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var comma = i < properties.Count - 1 ? "," : string.Empty;
            var ordinal = prop.Ordinal ?? i;

            if (prop.IsNullable)
            {
                var baseType = GetBaseType(prop.PropertyType);

                if (IsStringType(baseType))
                {
                    sb.AppendLine($"            {prop.PropertyName} = reader.GetNullableString({ordinal}){comma}");
                }
                else
                {
                    var clrType = GetClrTypeName(baseType);
                    sb.AppendLine($"            {prop.PropertyName} = reader.GetNullableValue<{clrType}>({ordinal}){comma}");
                }
            }
            else
            {
                var fastMethod = GetFastMethod(prop.PropertyType);
                sb.AppendLine($"            {prop.PropertyName} = reader.{fastMethod}({ordinal}){comma}");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
    }

    private static void GenerateFastMapMethodWithCachedOrdinals(
        StringBuilder sb,
        string className,
        IList<PropertyColumnInfo> properties)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Fastest mapping using cached ordinals. Call CacheOrdinals() first.");
        sb.AppendLine("    /// Use this when column order may differ from property order.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    [SkipLocalsInit]");
        sb.AppendLine($"    public {className} MapWithCachedOrdinals(SqlDataReader reader)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {className}");
        sb.AppendLine("        {");

        for (var i = 0; i < properties.Count; i++)
        {
            var prop = properties[i];
            var comma = i < properties.Count - 1 ? "," : string.Empty;

            if (prop.IsNullable)
            {
                var baseType = GetBaseType(prop.PropertyType);

                if (IsStringType(baseType))
                {
                    sb.AppendLine($"            {prop.PropertyName} = reader.GetNullableString(Ordinals.{prop.PropertyName}){comma}");
                }
                else
                {
                    var clrType = GetClrTypeName(baseType);
                    sb.AppendLine($"            {prop.PropertyName} = reader.GetNullableValue<{clrType}>(Ordinals.{prop.PropertyName}){comma}");
                }
            }
            else
            {
                var fastMethod = GetFastMethod(prop.PropertyType);
                sb.AppendLine($"            {prop.PropertyName} = reader.{fastMethod}(Ordinals.{prop.PropertyName}){comma}");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
    }

    private static string GetReaderMethodInternal(string propertyType)
    {
        var baseType = GetBaseType(propertyType);

        if (TypeToReaderMethod.TryGetValue(baseType, out var method))
        {
            return method;
        }

        return "GetValue";
    }

    private static string GetFastMethod(string propertyType)
    {
        var baseType = GetBaseType(propertyType);

        if (TypeToFastMethod.TryGetValue(baseType, out var method))
        {
            return method;
        }

        return "GetFieldValue<object>";
    }

    private static string GetClrTypeName(string propertyType)
    {
        return propertyType switch
        {
            "bool" => "bool",
            "System.Boolean" => "bool",
            "byte" => "byte",
            "System.Byte" => "byte",
            "short" => "short",
            "System.Int16" => "short",
            "int" => "int",
            "System.Int32" => "int",
            "long" => "long",
            "System.Int64" => "long",
            "float" => "float",
            "System.Single" => "float",
            "double" => "double",
            "System.Double" => "double",
            "decimal" => "decimal",
            "System.Decimal" => "decimal",
            "System.DateTime" => "DateTime",
            "System.Guid" => "Guid",
            "char" => "char",
            "System.Char" => "char",
            _ => propertyType,
        };
    }

    private static bool IsStringType(string propertyType)
    {
        return string.Equals(propertyType, "string", StringComparison.Ordinal)
            || string.Equals(propertyType, "System.String", StringComparison.Ordinal);
    }

    private static string GetBaseType(string propertyType)
    {
        if (propertyType.EndsWith("?", StringComparison.Ordinal))
        {
            return propertyType.Substring(0, propertyType.Length - 1);
        }

        if (propertyType.StartsWith("System.Nullable<", StringComparison.Ordinal))
        {
            return propertyType.Substring(16, propertyType.Length - 17);
        }

        return propertyType;
    }

    private static void GenerateOrdinalCache(
        StringBuilder sb,
        IList<PropertyColumnInfo> properties)
    {
        sb.AppendLine("    // Cached ordinals for maximum performance when column order may vary");
        sb.AppendLine("    private static class Ordinals");
        sb.AppendLine("    {");

        var ordinalIndex = 0;
        foreach (var prop in properties)
        {
            var ordinal = prop.Ordinal ?? ordinalIndex;
            sb.AppendLine($"        public static int {prop.PropertyName} = {ordinal};");
            ordinalIndex++;
        }

        sb.AppendLine("        public static volatile bool IsInitialized;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Caches column ordinals from the reader for subsequent reads.");
        sb.AppendLine("    /// Call once per query result set for best performance.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    public static void CacheOrdinals(SqlDataReader reader)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (Ordinals.IsInitialized) return;");

        foreach (var prop in properties)
        {
            var columnName = prop.ColumnName ?? prop.PropertyName;
            sb.AppendLine($"        Ordinals.{prop.PropertyName} = reader.GetOrdinal(\"{columnName}\");");
        }

        sb.AppendLine("        Ordinals.IsInitialized = true;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Resets cached ordinals. Call when switching to a different query.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static void ResetOrdinals()");
        sb.AppendLine("    {");
        sb.AppendLine("        Ordinals.IsInitialized = false;");
        sb.AppendLine("    }");
    }
}
