'use client';

import { useEffect, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input }  from '@/components/ui/input';
import { Badge }  from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useHistory } from '@/hooks/useHistory';
import { formatDate, getStatusColor } from '@/lib/utils';
import { History, ChevronLeft, ChevronRight, Loader2 } from 'lucide-react';

export function HistoryTable() {
  const { fetchHistory, history, historyLoading } = useHistory();
  const [plate,    setPlate]    = useState('');
  const [page,     setPage]     = useState(1);
  const pageSize = 10;

  // Cargar al montar y cuando cambian los filtros
  useEffect(() => {
    fetchHistory(plate || undefined, page, pageSize);
  }, [page]);

  const handleSearch = () => {
    setPage(1);
    fetchHistory(plate || undefined, 1, pageSize);
  };

  const totalPages = history
    ? Math.ceil(history.total / pageSize)
    : 0;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <History className="w-5 h-5" />
          Historial de consultas
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Filtro por placa */}
        <div className="flex gap-2">
          <Input
            placeholder="Filtrar por placa..."
            value={plate}
            onChange={(e) => setPlate(e.target.value.toUpperCase())}
            maxLength={6}
            className="uppercase"
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
          />
          <Button onClick={handleSearch} variant="outline">
            Buscar
          </Button>
        </div>

        {/* Tabla */}
        {historyLoading ? (
          <div className="flex justify-center py-8">
            <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
          </div>
        ) : history?.items.length === 0 ? (
          <p className="text-center text-muted-foreground py-8">
            No hay consultas registradas.
          </p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2 px-2 font-medium">Placa</th>
                  <th className="text-left py-2 px-2 font-medium">Fecha</th>
                  <th className="text-left py-2 px-2 font-medium">Tipo</th>
                  <th className="text-left py-2 px-2 font-medium">Estado</th>
                  <th className="text-right py-2 px-2 font-medium">Multas</th>
                </tr>
              </thead>
              <tbody>
                {history?.items.map((item) => (
                  <tr key={item.id} className="border-b hover:bg-muted/50">
                    <td className="py-2 px-2 font-mono font-medium">
                      {item.plate}
                    </td>
                    <td className="py-2 px-2 text-muted-foreground">
                      {formatDate(item.consultedAt)}
                    </td>
                    <td className="py-2 px-2">
                      <Badge variant="outline" className="text-xs">
                        {item.queryType}
                      </Badge>
                    </td>
                    <td className="py-2 px-2">
                      <Badge className={getStatusColor(item.status)}>
                        {item.status}
                      </Badge>
                    </td>
                    <td className="py-2 px-2 text-right">
                      {item.finesCount > 0 ? (
                        <span className="text-destructive font-medium">
                          {item.finesCount}
                        </span>
                      ) : (
                        <span className="text-muted-foreground">0</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Paginación */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              Página {page} de {totalPages} ({history?.total} registros)
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1 || historyLoading}
              >
                <ChevronLeft className="w-4 h-4" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages || historyLoading}
              >
                <ChevronRight className="w-4 h-4" />
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}