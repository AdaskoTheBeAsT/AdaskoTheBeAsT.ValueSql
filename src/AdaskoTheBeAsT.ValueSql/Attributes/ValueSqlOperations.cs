using System;

namespace AdaskoTheBeAsT.ValueSql.Attributes;

/// <summary>
/// Specifies which data operations to generate for a repository.
/// </summary>
[Flags]
public enum ValueSqlOperations
{
    /// <summary>
    /// No operations generated.
    /// </summary>
    None = 0,

    /// <summary>
    /// Generate GetById and GetAll methods.
    /// </summary>
    Get = 1,

    /// <summary>
    /// Generate single row Insert method.
    /// </summary>
    Insert = 2,

    /// <summary>
    /// Generate single row Update method.
    /// </summary>
    Update = 4,

    /// <summary>
    /// Generate single row Delete method.
    /// </summary>
    Delete = 8,

    /// <summary>
    /// Generate bulk Insert using TVP.
    /// </summary>
    BulkInsert = 16,

    /// <summary>
    /// Generate bulk Update using TVP.
    /// </summary>
    BulkUpdate = 32,

    /// <summary>
    /// Generate bulk Delete using TVP.
    /// </summary>
    BulkDelete = 64,

    /// <summary>
    /// Generate single row Merge (upsert) method.
    /// </summary>
    Merge = 128,

    /// <summary>
    /// Generate bulk Merge (upsert) using TVP.
    /// </summary>
    BulkMerge = 256,

    /// <summary>
    /// Generate custom query methods.
    /// </summary>
    Query = 512,

    /// <summary>
    /// Read-only operations (Get + Query).
    /// </summary>
    ReadOnly = Get | Query,

    /// <summary>
    /// Standard CRUD operations (Get + Insert + Update + Delete).
    /// </summary>
    Crud = Get | Insert | Update | Delete,

    /// <summary>
    /// All bulk operations.
    /// </summary>
    Bulk = BulkInsert | BulkUpdate | BulkDelete | BulkMerge,

    /// <summary>
    /// All operations.
    /// </summary>
    All = Crud | Bulk | Merge | Query,
}
