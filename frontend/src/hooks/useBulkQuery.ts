import { isAxiosError } from 'axios';
import { useCallback } from 'react';
import { toast } from 'sonner';
import { solveCaptcha } from '@/lib/captcha';
import { bulkQueryService } from '@/service/simitService';
import { useSimitStore } from '@/store/simitStore';

/**
 * Hook para consulta masiva de placas.
 * Un solo token captcha para todo el lote.
 */
export const useBulkQuery = () => {
  const {
    setBulkResult,
    setBulkLoading,
    setBulkError,
    bulkLoading,
    bulkResult,
    bulkError,
  } = useSimitStore();

  const bulkQuery = useCallback(async (plates: string[]) => {
    setBulkLoading(true);
    setBulkError(null);
    setBulkResult(null);

    const toastId = toast.loading('Resolviendo captcha...');

    try {
      const captchaToken = await solveCaptcha();

      toast.loading(
        `Consultando ${plates.length} placas...`,
        { id: toastId }
      );

      const result = await bulkQueryService({ plates, captchaToken });

      setBulkResult(result);

      toast.success(
        `Lote completado: ${result.successful}/${result.totalProcessed} exitosas`,
        { id: toastId }
      );

      return result;
    } catch (error: unknown) {
      const message = isAxiosError(error)
        ? error.response?.data?.message || 'Error en consulta masiva'
        : 'Error inesperado';

      setBulkError(message);
      toast.error(message, { id: toastId });
    } finally {
      setBulkLoading(false);
    }
  }, [setBulkLoading, setBulkError, setBulkResult]);

  return { bulkQuery, bulkLoading, bulkResult, bulkError };
};