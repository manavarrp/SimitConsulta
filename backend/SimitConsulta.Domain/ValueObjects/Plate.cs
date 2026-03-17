using SimitConsulta.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace SimitConsulta.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa una placa vehicular colombiana.
    /// Formatos válidos:
    ///   Carro: 3 letras + 3 dígitos  → ABC123
    ///   Moto:  3 letras + 2 dígitos + 1 letra → ABC12D
    ///
    /// Constructor privado: la única forma de obtener una instancia
    /// es vía Placa.Create(), que valida antes de instanciar.
    /// Es imposible tener un Placa con valor inválido.
    /// </summary>
    public sealed class Plate : IEquatable<Plate>
    {
        private static readonly Regex _regex =
            new(@"^[A-Z]{3}[0-9]{2}[A-Z0-9]{1}$", RegexOptions.Compiled);

        /// <summary>Valor normalizado (mayúsculas, sin espacios).</summary>
        public string Value { get; }

        private Plate(string value) => Value = value;

        /// <summary>
        /// Crea una instancia válida de Placa.
        /// Normaliza automáticamente: elimina espacios y convierte a mayúsculas.
        /// </summary>
        /// <exception cref="DomainException">Si el formato no es válido.</exception>
        public static Plate Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new DomainException("La placa no puede estar vacía.");

            var normalized = input.Trim().ToUpperInvariant();

            if (!_regex.IsMatch(normalized))
                throw new DomainException(
                    $"Formato de placa inválido: '{input}'. " +
                    "Ejemplos válidos: ABC123 (carro), ABC12D (moto).");

            return new Plate(normalized);
        }

        /// <summary>
        /// Intenta crear sin lanzar excepción.
        /// Usado en FluentValidation donde las excepciones son inadecuadas.
        /// </summary>
        public static bool TryCreate(string input, out Plate? plate, out string error)
        {
            plate = null;
            error = string.Empty;
            try
            {
                plate = Create(input);
                return true;
            }
            catch (DomainException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool Equals(Plate? other) => other is not null && Value == other.Value;
        public override bool Equals(object? obj) => obj is Plate p && Equals(p);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        /// <summary>Conversión implícita para uso natural en LINQ y logs.</summary>
        public static implicit operator string(Plate p) => p.Value;
    }
}
