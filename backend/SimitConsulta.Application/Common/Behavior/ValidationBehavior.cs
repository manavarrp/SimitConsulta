using FluentValidation;
using MediatR;

namespace SimitConsulta.Application.Common.Behaviors;

/// <summary>
/// Ejecuta automáticamente los validators de FluentValidation
/// para cada request en el pipeline de MediatR.
///
/// Si hay errores:
/// → Lanza ValidationException
/// → Corta la ejecución (no llega al handler)
///
/// Se maneja en middleware global.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Si no hay validators → sigue normal
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        // Ejecutar todos los validators (soporta async)
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // 🚨 Si hay errores → cortar pipeline
        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}