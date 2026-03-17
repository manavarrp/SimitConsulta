namespace SimitConsulta.Application.Common.Results;

/// <summary>
/// Contenedor de resultado que evita usar excepciones para flujo de control.
/// Cada operación retorna Ok(valor) o Fail(error) en lugar de lanzar.
///
/// Las excepciones se reservan para errores reales de infraestructura.
/// Los errores esperados (validación, placa no encontrada) usan Result.Fail.
/// </summary>
public class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value) { Value = value; IsSuccess = true; }
    private Result(string error) { Error = error; IsSuccess = false; }

    /// <summary>Crea un resultado exitoso con el valor indicado.</summary>
    public static Result<T> Ok(T value) => new(value);

    /// <summary>Crea un resultado fallido con el mensaje de error.</summary>
    public static Result<T> Fail(string error) => new(error);

    /// <summary>
    /// Transforma el valor si es exitoso.
    /// Si es fallido propaga el error sin ejecutar el mapper.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess
            ? Result<TOut>.Ok(mapper(Value!))
            : Result<TOut>.Fail(Error!);

    /// <summary>
    /// Encadena operaciones async que también retornan Result.
    /// Cortocircuita si el resultado actual es fallido.
    /// </summary>
    public async Task<Result<TOut>> BindAsync<TOut>(
        Func<T, Task<Result<TOut>>> next) =>
        IsSuccess
            ? await next(Value!)
            : Result<TOut>.Fail(Error!);

    public override string ToString() =>
        IsSuccess ? $"Ok({Value})" : $"Fail({Error})";
}

/// <summary>
/// Versión sin valor de retorno para comandos void.
/// Provee los factory methods estáticos tipados.
/// </summary>
public class Result
{
    public string? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(bool success, string? error)
    {
        IsSuccess = success;
        Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string error) => new(false, error);
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(string e) => Result<T>.Fail(e);
}