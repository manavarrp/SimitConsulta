import axios from '@/axios';
import {
  BulkQueryDto,
  BulkQueryRequest,
  HistoryDto,
  PlateQueryDto,
  QueryPlateRequest,
} from '@/interfaces';

/**
 * Consulta multas y comparendos de una placa en el SIMIT.
 * El captchaToken debe ser resuelto antes de llamar este servicio.
 */
export const queryPlateService = async (
  params: QueryPlateRequest
): Promise<PlateQueryDto> => {
  const response = await axios.post<PlateQueryDto>('/query', params);
  console.log("queryPlateService", response);
  return response.data;
};

/**
 * Consulta un lote de placas en paralelo.
 * Un único token captcha para todo el lote.
 */
export const bulkQueryService = async (
  params: BulkQueryRequest
): Promise<BulkQueryDto> => {
  const response = await axios.post<BulkQueryDto>('/query/bulk', params);
  console.log("bulkQueryService", response);
  return response.data;
};

/**
 * Obtiene el historial paginado de consultas.
 */
export const getHistoryService = async (
  plate?:    string,
  page:      number = 1,
  pageSize:  number = 20
): Promise<HistoryDto> => {
  const response = await axios.get<HistoryDto>('/history', {
    params: { plate, page, pageSize },
  });
  console.log("getHistoryService", response);

  return response.data;
};