using System.Collections.Generic;
using System.Text;
using AdaskoTheBeAsT.ValueSql.SourceGenerator;

namespace AdaskoTheBeAsT.ValueSql.SqlServer;

/// <summary>
/// Generates SQL Server MERGE statement and extension methods.
/// </summary>
public static class SqlServerMergeGenerator
{
    public static string GenerateMergeSql(
        string schema,
        string targetTable,
        string targetAlias,
        string sourceAlias,
        string parameterName,
        IList<string> matchColumns,
        IList<string> updateColumns,
        IList<string> insertColumns,
        int operations,
        string? updateCondition,
        string? deleteCondition,
        bool outputAction,
        bool outputInserted,
        bool outputDeleted)
    {
        var sb = new StringBuilder(512);
        var fullTableName = string.IsNullOrEmpty(schema) ? targetTable : $"{schema}.{targetTable}";

        sb.AppendLine($"MERGE INTO {fullTableName} AS {targetAlias}");
        sb.AppendLine($"USING {parameterName} AS {sourceAlias}");

        AppendOnClause(sb, targetAlias, sourceAlias, matchColumns);
        AppendUpdateClause(sb, targetAlias, sourceAlias, updateColumns, operations, updateCondition);
        AppendInsertClause(sb, sourceAlias, insertColumns, operations);
        AppendDeleteClause(sb, operations, deleteCondition);
        AppendOutputClause(sb, outputAction, outputInserted, outputDeleted);

        sb.Append(';');
        return sb.ToString();
    }

    public static string GenerateMergeExtensionClass(
        string namespaceName,
        string className,
        string entityType,
        string mergeSql,
        string tvpTypeName,
        string parameterName,
        bool generateAsync,
        bool generateSync,
        bool hasOutput,
        string? outputType)
    {
        var sb = new StringBuilder(2048);
        var mapperName = $"{className}TvpMapper";
        var extensionClassName = $"{className}MergeExtensions";

        GenerateExtensionHeader(sb, namespaceName, className, extensionClassName, mergeSql);

        if (generateAsync)
        {
            GenerateMergeAsyncMethod(sb, className, entityType, mapperName, tvpTypeName, parameterName, hasOutput, outputType);
        }

        if (generateSync)
        {
            if (generateAsync)
            {
                sb.AppendLine();
            }

            GenerateMergeSyncMethod(sb, className, entityType, mapperName, tvpTypeName, parameterName, hasOutput, outputType);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void AppendOnClause(StringBuilder sb, string targetAlias, string sourceAlias, IList<string> matchColumns)
    {
        sb.Append("ON ");
        for (var i = 0; i < matchColumns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(" AND ");
            }

            sb.Append($"{targetAlias}.{matchColumns[i]} = {sourceAlias}.{matchColumns[i]}");
        }

        sb.AppendLine();
    }

    private static void AppendUpdateClause(
        StringBuilder sb,
        string targetAlias,
        string sourceAlias,
        IList<string> updateColumns,
        int operations,
        string? updateCondition)
    {
        if ((operations & 2) == 0 || updateColumns.Count == 0)
        {
            return;
        }

        sb.Append("WHEN MATCHED");
        if (!string.IsNullOrEmpty(updateCondition))
        {
            sb.Append($" AND {updateCondition}");
        }

        sb.AppendLine(" THEN");
        sb.Append("    UPDATE SET ");
        for (var i = 0; i < updateColumns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append($"{targetAlias}.{updateColumns[i]} = {sourceAlias}.{updateColumns[i]}");
        }

        sb.AppendLine();
    }

    private static void AppendInsertClause(StringBuilder sb, string sourceAlias, IList<string> insertColumns, int operations)
    {
        if ((operations & 1) == 0 || insertColumns.Count == 0)
        {
            return;
        }

        sb.AppendLine("WHEN NOT MATCHED BY TARGET THEN");
        sb.Append("    INSERT (");
        sb.Append(string.Join(", ", insertColumns));
        sb.Append(") VALUES (");
        for (var i = 0; i < insertColumns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append($"{sourceAlias}.{insertColumns[i]}");
        }

        sb.Append(')');
        sb.AppendLine();
    }

    private static void AppendDeleteClause(StringBuilder sb, int operations, string? deleteCondition)
    {
        if ((operations & 4) == 0)
        {
            return;
        }

        sb.Append("WHEN NOT MATCHED BY SOURCE");
        if (!string.IsNullOrEmpty(deleteCondition))
        {
            sb.Append($" AND {deleteCondition}");
        }

        sb.AppendLine(" THEN");
        sb.AppendLine("    DELETE");
    }

    private static void AppendOutputClause(StringBuilder sb, bool outputAction, bool outputInserted, bool outputDeleted)
    {
        if (!outputAction && !outputInserted && !outputDeleted)
        {
            return;
        }

        sb.Append("OUTPUT ");
        var hasOutput = false;

        if (outputAction)
        {
            sb.Append("$action AS MergeAction");
            hasOutput = true;
        }

        if (outputInserted)
        {
            if (hasOutput)
            {
                sb.Append(", ");
            }

            sb.Append("INSERTED.*");
            hasOutput = true;
        }

        if (outputDeleted)
        {
            if (hasOutput)
            {
                sb.Append(", ");
            }

            sb.Append("DELETED.*");
        }

        sb.AppendLine();
    }

    private static void GenerateExtensionHeader(
        StringBuilder sb,
        string namespaceName,
        string className,
        string extensionClassName,
        string mergeSql)
    {
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Data;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Data.SqlClient;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// MERGE extension methods for {className}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {extensionClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    private const string MergeSql = \"\"\"");
        sb.AppendLine($"        {mergeSql.Replace("\r\n", "\r\n        ")}");
        sb.AppendLine("        \"\"\";");
        sb.AppendLine();
    }

    private static void GenerateMergeAsyncMethod(
        StringBuilder sb,
        string className,
        string entityType,
        string mapperName,
        string tvpTypeName,
        string parameterName,
        bool hasOutput,
        string? outputType)
    {
        var returnType = hasOutput ? $"Task<IReadOnlyList<{outputType}>>" : "Task<int>";
        var methodName = hasOutput ? $"MergeWithOutputAsync" : "MergeAsync";

        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Executes MERGE operation for {className} items.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static async {returnType} {methodName}(");
        sb.AppendLine("        this SqlConnection connection,");
        sb.AppendLine($"        IEnumerable<{entityType}> items,");
        sb.AppendLine("        SqlTransaction? transaction = null,");
        sb.AppendLine("        int commandTimeout = 30,");
        sb.AppendLine("        CancellationToken ct = default)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var mapper = default({mapperName});");
        sb.AppendLine("        await using var cmd = connection.CreateCommand();");
        sb.AppendLine("        cmd.CommandText = MergeSql;");
        sb.AppendLine("        cmd.CommandTimeout = commandTimeout;");
        sb.AppendLine("        cmd.Transaction = transaction;");
        sb.AppendLine();
        sb.AppendLine($"        var param = new SqlParameter(\"{parameterName}\", SqlDbType.Structured)");
        sb.AppendLine("        {");
        sb.AppendLine($"            TypeName = \"{tvpTypeName}\",");
        sb.AppendLine("            Value = mapper.ToSqlDataRecords(items),");
        sb.AppendLine("        };");
        sb.AppendLine("        cmd.Parameters.Add(param);");
        sb.AppendLine();

        if (hasOutput)
        {
            sb.AppendLine($"        var results = new List<{outputType}>();");
            sb.AppendLine("        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);");
            sb.AppendLine("        while (await reader.ReadAsync(ct).ConfigureAwait(false))");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Map reader to output type");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return results;");
        }
        else
        {
            sb.AppendLine("        return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);");
        }

        sb.AppendLine("    }");
    }

    private static void GenerateMergeSyncMethod(
        StringBuilder sb,
        string className,
        string entityType,
        string mapperName,
        string tvpTypeName,
        string parameterName,
        bool hasOutput,
        string? outputType)
    {
        var returnType = hasOutput ? $"IReadOnlyList<{outputType}>" : "int";
        var methodName = hasOutput ? "MergeWithOutput" : "Merge";

        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Executes MERGE operation for {className} items (synchronous).");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static {returnType} {methodName}(");
        sb.AppendLine("        this SqlConnection connection,");
        sb.AppendLine($"        IEnumerable<{entityType}> items,");
        sb.AppendLine("        SqlTransaction? transaction = null,");
        sb.AppendLine("        int commandTimeout = 30)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var mapper = default({mapperName});");
        sb.AppendLine("        using var cmd = connection.CreateCommand();");
        sb.AppendLine("        cmd.CommandText = MergeSql;");
        sb.AppendLine("        cmd.CommandTimeout = commandTimeout;");
        sb.AppendLine("        cmd.Transaction = transaction;");
        sb.AppendLine();
        sb.AppendLine($"        var param = new SqlParameter(\"{parameterName}\", SqlDbType.Structured)");
        sb.AppendLine("        {");
        sb.AppendLine($"            TypeName = \"{tvpTypeName}\",");
        sb.AppendLine("            Value = mapper.ToSqlDataRecords(items),");
        sb.AppendLine("        };");
        sb.AppendLine("        cmd.Parameters.Add(param);");
        sb.AppendLine();

        if (hasOutput)
        {
            sb.AppendLine($"        var results = new List<{outputType}>();");
            sb.AppendLine("        using var reader = cmd.ExecuteReader();");
            sb.AppendLine("        while (reader.Read())");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Map reader to output type");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return results;");
        }
        else
        {
            sb.AppendLine("        return cmd.ExecuteNonQuery();");
        }

        sb.AppendLine("    }");
    }
}
