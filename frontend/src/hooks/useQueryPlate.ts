import { isAxiosError } from 'axios';
import { useCallback } from 'react';
import { toast } from 'sonner';
import { solveCaptcha } from '@/lib/captcha';
import { queryPlateService } from '@/service/simitService';
import { useSimitStore } from '@/store/simitStore';

/**
 * Hook para consulta individual de placa.
 * Resuelve el captcha automáticamente antes de llamar al backend.
 */
export const useQueryPlate = () => {
  const {
    setQueryResult,
    setQueryLoading,
    setQueryError,
    queryLoading,
    queryResult,
    queryError,
  } = useSimitStore();

  const queryPlate = useCallback(async (plate: string) => {
    setQueryLoading(true);
    setQueryError(null);
    setQueryResult(null);

    const toastId = toast.loading('Resolviendo captcha...');

    try {
      // 1. Resolver captcha en el navegador
      const captchaToken = await solveCaptcha();
      console.log('Token enviado al backend:', captchaToken);
      toast.loading('Consultando SIMIT...', { id: toastId });

      // 2. Llamar al backend con el token
      const result = await queryPlateService({ plate, captchaToken });

      setQueryResult(result);

      if (result.status === 'Exitoso') {
        toast.success(
          `Placa ${plate}: ${result.finesCount} multa(s) encontrada(s)`,
          { id: toastId }
        );
      } else if (result.status === 'SinMultas') {
        toast.success(
          `Placa ${plate}: sin multas registradas`,
          { id: toastId }
        );
      } else {
        toast.error(
          result.errorMessage || 'Error al consultar la placa',
          { id: toastId }
        );
      }

      return result;
    } catch (error: unknown) {
      const message = isAxiosError(error)
        ? error.response?.data?.message || 'Error al consultar'
        : 'Error inesperado';

      setQueryError(message);
      toast.error(message, { id: toastId });
    } finally {
      setQueryLoading(false);
    }
  }, [setQueryLoading, setQueryError, setQueryResult]);

  return { queryPlate, queryLoading, queryResult, queryError };
};