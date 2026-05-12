using eMarketing.Service.Dtos;
using Microsoft.Data.SqlClient;

namespace eMarketing.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Message, new[] { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Message, new[] { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status403Forbidden, ex.Message, new[] { ex.Message });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while processing {Method} {Path}. TraceId: {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "İşlem sırasında hata oluştu.", Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while processing {Method} {Path}. TraceId: {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "İşlem sırasında hata oluştu.", Array.Empty<string>());
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message, IReadOnlyList<string> errors)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Message = message,
            TraceId = context.TraceIdentifier,
            Errors = errors.Cast<object>().ToArray()
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
