using System.Collections.Generic;

namespace AdaskoTheBeAsT.ValueSql.SourceGenerator.Abstractions;

public interface IMapperClassGenerator
{
    string Generate(
        string namespaceName,
        string className,
        IList<PropertyColumnInfo> properties);
}
