using System.Data;
using eMarketing.Service.Connection;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Services;

public interface ISqlDataService
{
    Task<IReadOnlyList<Dictionary<string, object?>>> QueryAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object?>?> QuerySingleAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);
    Task<int> ExecuteScalarIntAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);
}

public sealed class SqlDataService : ISqlDataService
{
    private readonly ISqlExecutor _sqlExecutor;

    public SqlDataService(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> QueryAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        return await _sqlExecutor.QueryAsync(procedureName, parameters, reader =>
        {
            var row = new Dictionary<string, object?>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[reader.GetName(i)] = value;
            }

            return row;
        }, cancellationToken);
    }

    public async Task<Dictionary<string, object?>?> QuerySingleAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Dictionary<string, object?>> rows = await QueryAsync(procedureName, parameters, cancellationToken);
        return rows.Count == 0 ? null : rows[0];
    }

    public async Task<int> ExecuteAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        return await _sqlExecutor.ExecuteAsync(procedureName, parameters, cancellationToken);
    }

    public async Task<int> ExecuteScalarIntAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        return await _sqlExecutor.ExecuteScalarIntAsync(procedureName, parameters, cancellationToken);
    }

    public static SqlParameter Param(string name, SqlDbType type, object? value)
    {
        return SqlParameterFactory.Param(name, type, value);
    }

    public static SqlParameter TextParam(string name, int size, string? value)
    {
        return SqlParameterFactory.TextParam(name, size, value);
    }

    public static SqlParameter NullableTextParam(string name, int size, string? value)
    {
        return SqlParameterFactory.NullableTextParam(name, size, value);
    }
}
