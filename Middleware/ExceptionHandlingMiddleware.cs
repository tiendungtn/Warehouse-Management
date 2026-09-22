using System.Net;
using System.Text.Json;

namespace QuanLyKho.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(
                context,
                HttpStatusCode.NotFound,
                ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorAsync(
                context,
                HttpStatusCode.Forbidden,
                ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteErrorAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled API exception");

            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Đã xảy ra lỗi không mong muốn.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)statusCode;

        context.Response.ContentType =
            "application/json; charset=utf-8";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new
            {
                success = false,
                message
            }));
    }
}