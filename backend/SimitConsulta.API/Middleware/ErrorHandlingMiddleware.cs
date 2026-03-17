using System.Text.Json;
using FluentValidation;
using SimitConsulta.Domain.Exceptions;

namespace SimitConsulta.API.Middleware;

/// <summary>
/// Middleware global que captura excepciones no controladas
/// y las convierte en respuestas JSON con el HTTP status correcto.
///
/// Los errores controlados (Result.Fail) NO llegan aquí —
/// el controller los maneja directamente.
/// Este middleware solo captura lo que escapa del flujo normal.
///
/// Mapeo:
/// - ValidationException  → 400 (validación que no pasó por Result)
/// - DomainException      → 400 (regla de negocio violada)
/// - SimitGatewayException → 502 (SIMIT no responde)
/// - OperationCanceled    → 499 (cliente canceló la petición)
/// - Cualquier otra       → 500 (error inesperado)
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Envuelve todo el pipeline en un try/catch.
    /// Si ocurre una excepción la convierte en JSON.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            // Validación que no pasó por Result<T>
            ValidationException ve => (400,
                string.Join(" | ",
                    ve.Errors.Select(e => e.ErrorMessage))),

            // SimitGatewayException ANTES que DomainException
            // porque hereda de ella — más específico primero
            SimitGatewayException => (502, ex.Message),

            // Regla de negocio violada — base, va después
            DomainException => (400, ex.Message),

            // Cliente canceló la petición
            OperationCanceledException => (499,
                "Request cancelled by client."),

            // Error inesperado
            _ => (500, "Internal server error.")
        };

        context.Response.StatusCode = statusCode;

        var body = JsonSerializer.Serialize(new
        {
            error = true,
            code = statusCode,
            message
        });

        await context.Response.WriteAsync(body);
    }
}