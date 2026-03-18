import { create } from 'zustand';
import { BulkQueryDto, HistoryDto, PlateQueryDto } from '@/interfaces';

interface SimitState {
  // Consulta individual
  queryResult:    PlateQueryDto | null;
  queryLoading:   boolean;
  queryError:     string | null;
  setQueryResult: (result: PlateQueryDto | null) => void;
  setQueryLoading:(loading: boolean) => void;
  setQueryError:  (error: string | null) => void;

  // Consulta masiva
  bulkResult:     BulkQueryDto | null;
  bulkLoading:    boolean;
  bulkError:      string | null;
  setBulkResult:  (result: BulkQueryDto | null) => void;
  setBulkLoading: (loading: boolean) => void;
  setBulkError:   (error: string | null) => void;

  // Historial
  history:        HistoryDto | null;
  historyLoading: boolean;
  historyError:   string | null;
  setHistory:     (history: HistoryDto | null) => void;
  setHistoryLoading:(loading: boolean) => void;
  setHistoryError:(error: string | null) => void;

  // Pestaña activa
  activeTab:      string;
  setActiveTab:   (tab: string) => void;

  // Reset
  resetAll:       () => void;
}

export const useSimitStore = create<SimitState>((set) => ({
  // Consulta individual
  queryResult:    null,
  queryLoading:   false,
  queryError:     null,
  setQueryResult: (result)  => set({ queryResult: result }),
  setQueryLoading:(loading) => set({ queryLoading: loading }),
  setQueryError:  (error)   => set({ queryError: error }),

  // Consulta masiva
  bulkResult:     null,
  bulkLoading:    false,
  bulkError:      null,
  setBulkResult:  (result)  => set({ bulkResult: result }),
  setBulkLoading: (loading) => set({ bulkLoading: loading }),
  setBulkError:   (error)   => set({ bulkError: error }),

  // Historial
  history:        null,
  historyLoading: false,
  historyError:   null,
  setHistory:     (history) => set({ history: history }),
  setHistoryLoading:(loading) => set({ historyLoading: loading }),
  setHistoryError:(error)   => set({ historyError: error }),

  // Pestaña activa
  activeTab:      'individual',
  setActiveTab:   (tab)     => set({ activeTab: tab }),

  // Reset todo el estado
  resetAll: () => set({
    queryResult:  null,
    queryError:   null,
    bulkResult:   null,
    bulkError:    null,
    history:      null,
    historyError: null,
  }),
}));