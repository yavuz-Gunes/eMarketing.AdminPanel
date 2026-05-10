using System.Data;
using eMarketing.Service.Connection;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Services;

public interface ISqlExecutor
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string procedureName, IEnumerable<SqlParameter> parameters, Func<SqlDataReader, T> map, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleAsync<T>(string procedureName, IEnumerable<SqlParameter> parameters, Func<SqlDataReader, T> map, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);
    Task<int> ExecuteScalarIntAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default);
}

public sealed class SqlExecutor : ISqlExecutor
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SqlExecutor(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string procedureName, IEnumerable<SqlParameter> parameters, Func<SqlDataReader, T> map, CancellationToken cancellationToken = default)
    {
        var rows = new List<T>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = CreateCommand(procedureName, parameters, connection);

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            rows.Add(map(reader));

        return rows;
    }

    public async Task<T?> QuerySingleAsync<T>(string procedureName, IEnumerable<SqlParameter> parameters, Func<SqlDataReader, T> map, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = CreateCommand(procedureName, parameters, connection);

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? map(reader) : default;
    }

    public async Task<int> ExecuteAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = CreateCommand(procedureName, parameters, connection);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> ExecuteScalarIntAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = CreateCommand(procedureName, parameters, connection);

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static SqlCommand CreateCommand(string procedureName, IEnumerable<SqlParameter> parameters, SqlConnection connection)
    {
        var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        foreach (SqlParameter parameter in parameters)
            command.Parameters.Add(parameter);

        return command;
    }
}

public static class SqlParameterFactory
{
    public static SqlParameter Param(string name, SqlDbType type, object? value)
    {
        return new SqlParameter(name, type)
        {
            Value = value ?? DBNull.Value
        };
    }

    public static SqlParameter TextParam(string name, int size, string? value)
    {
        return new SqlParameter(name, SqlDbType.NVarChar, size)
        {
            Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()
        };
    }

    public static SqlParameter NullableTextParam(string name, int size, string? value)
    {
        return new SqlParameter(name, SqlDbType.NVarChar, size)
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim()
        };
    }
}
