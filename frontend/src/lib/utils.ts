import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/** Formatea un número como pesos colombianos */
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-CO', {
    style:    'currency',
    currency: 'COP',
    maximumFractionDigits: 0,
  }).format(amount);
}

/** Formatea una fecha ISO a formato legible */
export function formatDate(dateStr: string): string {
  return new Intl.DateTimeFormat('es-CO', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone:  'America/Bogota',  
  }).format(new Date(dateStr));
}

/** Color del badge según el estado */
export function getStatusColor(status: string): string {
  switch (status) {
    case 'Exitoso':    return 'bg-destructive text-destructive-foreground';
    case 'SinMultas':  return 'bg-green-500 text-white';
    case 'Error':      return 'bg-gray-500 text-white';
    case 'Procesando': return 'bg-yellow-500 text-white';
    default:           return 'bg-secondary text-secondary-foreground';
  }
}