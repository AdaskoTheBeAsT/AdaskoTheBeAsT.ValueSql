using System.Text;

namespace AdaskoTheBeAsT.ValueSql.SourceGenerator.Builders;

public sealed class AdvancedSqlBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();
    private string? _insertClause;
    private string? _valuesClause;
    private string? _selectClause;
    private string? _setClause;
    private string? _whereClause;

    public string RawSql => _sb.ToString();

    public AdvancedSqlBuilder Insert(string columns)
    {
        _insertClause = columns;
        return this;
    }

    public AdvancedSqlBuilder Values(string values)
    {
        _valuesClause = values;
        return this;
    }

    public AdvancedSqlBuilder Select(string columns)
    {
        _selectClause = columns;
        return this;
    }

    public AdvancedSqlBuilder Set(string setClause)
    {
        _setClause = setClause;
        return this;
    }

    public AdvancedSqlBuilder Where(string whereClause)
    {
        _whereClause = whereClause;
        return this;
    }

    public AdvancedSqlBuilder AddTemplate(string template)
    {
        var result = template;

        if (_insertClause != null)
        {
            result = result.Replace("/**insert**/", _insertClause);
        }

        if (_valuesClause != null)
        {
            result = result.Replace("/**values**/", _valuesClause);
        }

        if (_selectClause != null)
        {
            result = result.Replace("/**select**/", _selectClause);
        }

        if (_setClause != null)
        {
            result = result.Replace("/**set**/", _setClause);
        }

        if (_whereClause != null)
        {
            result = result.Replace("/**where**/", _whereClause);
        }

        _sb.Append(result);
        return this;
    }
}
