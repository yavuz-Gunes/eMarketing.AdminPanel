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
    private readonly ISqlConnectionFactory _connectionFactory;

    public SqlDataService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> QueryAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        var rows = new List<Dictionary<string, object?>>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = CreateCommand(procedureName, parameters, connection);

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[reader.GetName(i)] = value;
            }

            rows.Add(row);
        }

        return rows;
    }

    public async Task<Dictionary<string, object?>?> QuerySingleAsync(string procedureName, IEnumerable<SqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Dictionary<string, object?>> rows = await QueryAsync(procedureName, parameters, cancellationToken);
        return rows.Count == 0 ? null : rows[0];
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
        {
            command.Parameters.Add(parameter);
        }

        return command;
    }

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
