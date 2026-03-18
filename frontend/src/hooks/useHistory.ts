import { isAxiosError } from 'axios';
import { useCallback } from 'react';
import { toast } from 'sonner';
import { getHistoryService } from '@/service/simitService';
import { useSimitStore } from '@/store/simitStore';

/**
 * Hook para obtener el historial paginado de consultas.
 */
export const useHistory = () => {
  const {
    setHistory,
    setHistoryLoading,
    setHistoryError,
    historyLoading,
    history,
    historyError,
  } = useSimitStore();

  const fetchHistory = useCallback(async (
    plate?:   string,
    page:     number = 1,
    pageSize: number = 10
  ) => {
    setHistoryLoading(true);
    setHistoryError(null);

    try {
      const result = await getHistoryService(plate, page, pageSize);
      setHistory(result);
      return result;
    } catch (error: unknown) {
      const message = isAxiosError(error)
        ? error.response?.data?.message || 'Error al cargar historial'
        : 'Error inesperado';

      setHistoryError(message);
      toast.error(message);
    } finally {
      setHistoryLoading(false);
    }
  }, [setHistory, setHistoryLoading, setHistoryError]);

  return { fetchHistory, historyLoading, history, historyError };
};