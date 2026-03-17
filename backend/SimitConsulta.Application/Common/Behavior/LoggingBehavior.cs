using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SimitConsulta.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que mide el tiempo de ejecución de cada request.
/// Se registra primero — mide el tiempo total incluyendo validación.
/// Emite Warning si supera 3 segundos para detectar consultas lentas.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Executing {Request}", name);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > 3_000)
            _logger.LogWarning(
                "Slow request {Request} took {Ms}ms — review performance",
                name, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation(
                "{Request} completed in {Ms}ms",
                name, sw.ElapsedMilliseconds);

        return response;
    }
}