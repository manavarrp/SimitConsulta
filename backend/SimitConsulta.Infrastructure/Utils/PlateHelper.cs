using SimitConsulta.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace SimitConsulta.Infrastructure.Utils;

/// <summary>
/// Utilidades estáticas para normalización y parsing de placas.
/// Delega la validación al Value Object Plate — fuente única de verdad.
/// </summary>
public static class PlateHelper
{
    /// <summary>
    /// Normaliza una placa: elimina espacios y convierte a mayúsculas.
    /// No valida el formato.
    /// </summary>
    public static string Normalize(string plate) =>
        plate.Trim().ToUpperInvariant();

    /// <summary>
    /// Verifica si una placa tiene formato válido sin lanzar excepción.
    /// Delega al Value Object Plate.
    /// </summary>
    public static bool IsValid(string plate) =>
        Plate.TryCreate(plate, out _, out _);

    /// <summary>
    /// Separa una lista en válidas e inválidas.
    /// Útil para reportar al cliente qué placas se rechazaron en un lote.
    /// </summary>
    public static (List<string> Valid, List<string> Invalid) Filter(
        IEnumerable<string> plates)
    {
        var valid = new List<string>();
        var invalid = new List<string>();

        foreach (var p in plates)
        {
            if (IsValid(p)) valid.Add(Normalize(p));
            else invalid.Add(p);
        }

        return (valid, invalid);
    }

    /// <summary>
    /// Extrae placas válidas de texto libre con delimitadores mixtos.
    /// Acepta: saltos de línea, comas, punto y coma, espacios.
    /// Normaliza y deduplica automáticamente.
    /// </summary>
    public static List<string> ExtractFrom(string text) =>
        Regex.Split(text, @"[\n,;\s]+")
             .Select(Normalize)
             .Where(IsValid)
             .Distinct()
             .ToList();
}
