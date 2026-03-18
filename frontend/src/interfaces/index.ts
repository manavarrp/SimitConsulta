// ── Respuesta de consulta individual ─────────────────────
export interface FineDto {
  number:        string | null;
  amount:        number;
  status:        string | null;
  infractionDate: string | null;
  agency:        string | null;
  description:   string | null;
}

export interface SummonsDto {
  number:        string | null;
  amount:        number;
  status:        string | null;
  infractionDate: string | null;
  infraction:    string | null;
}

export interface PlateQueryDto {
  id:               number;
  plate:            string;
  consultedAt:      string;
  status:           string;   // "Exitoso" | "SinMultas" | "Error" | "Procesando"
  queryType:        string;
  finesCount:       number;
  summonsCount:     number;
  totalAmount:      number;
  fines:            FineDto[];
  summons:          SummonsDto[];
  errorMessage:     string | null;
}

// ── Respuesta de consulta masiva ──────────────────────────
export interface BulkQueryDto {
  totalProcessed: number;
  successful:     number;
  failed:         number;
  results:        PlateQueryDto[];
}

// ── Historial ─────────────────────────────────────────────
export interface HistoryItemDto {
  id:           number;
  plate:        string;
  consultedAt:  string;
  queryType:    string;
  status:       string;
  finesCount:   number;
  errorMessage: string | null;
}

export interface HistoryDto {
  total:    number;
  page:     number;
  pageSize: number;
  items:    HistoryItemDto[];
}

// ── Requests ──────────────────────────────────────────────
export interface QueryPlateRequest {
  plate:        string;
  captchaToken: string;
}

export interface BulkQueryRequest {
  plates:       string[];
  captchaToken: string;
}

// ── Error de API ──────────────────────────────────────────
export interface ApiError {
  error:   boolean;
  message: string;
}