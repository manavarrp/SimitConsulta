import { z } from 'zod';

const PLATE_REGEX = /^[A-Z]{3}[0-9]{2}[A-Z0-9]{1}$/;

export const queryPlateSchema = z.object({
  plate: z
    .string()
    .min(1, 'La placa es obligatoria.')
    .transform((v) => v.trim().toUpperCase())
    .refine(
      (v) => PLATE_REGEX.test(v),
      'Formato inválido. Ejemplos: ABC123 (carro), ABC12D (moto).'
    ),
});

// El form recibe un string — la transformación ocurre en el submit
export const bulkQuerySchema = z.object({
  plates: z
    .string()
    .min(1, 'Ingresa al menos una placa.'),
});

// Función para parsear el string de placas
export function parsePlates(input: string): string[] {
  return input
    .split(/[\n,;\s]+/)
    .map((p) => p.trim().toUpperCase())
    .filter((p) => p.length > 0);
}

// Validar el array resultante
export function validatePlates(plates: string[]): string | null {
  if (plates.length === 0)  return 'Ingresa al menos una placa válida.';
  if (plates.length > 100)  return 'Máximo 100 placas por consulta masiva.';
  const invalid = plates.filter((p) => !PLATE_REGEX.test(p));
  if (invalid.length > 0)
    return `Placas con formato inválido: ${invalid.join(', ')}`;
  return null;
}

export type QueryPlateForm = z.infer<typeof queryPlateSchema>;
export type BulkQueryForm  = z.infer<typeof bulkQuerySchema>;