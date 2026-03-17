using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimitConsulta.Infrastructure.Utils;

/// <summary>
/// Opciones y métodos de serialización JSON compartidos.
/// Centraliza la configuración para evitar inconsistencias
/// entre clases que serializan de forma independiente.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Opciones estándar:
    /// - PropertyNameCaseInsensitive: acepta cualquier capitalización.
    /// - IgnoreNullValues: no serializa propiedades nulas.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>Serializa un objeto a JSON string.</summary>
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, DefaultOptions);

    /// <summary>
    /// Deserializa un JSON string al tipo indicado.
    /// Retorna null si el JSON es inválido.
    /// </summary>
    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultOptions);

    /// <summary>
    /// Intenta deserializar sin lanzar excepción.
    /// Útil para respuestas externas con formato inesperado.
    /// </summary>
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        result = default;
        try
        {
            result = Deserialize<T>(json);
            return result is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}