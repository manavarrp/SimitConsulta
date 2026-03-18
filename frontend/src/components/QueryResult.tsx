'use client';

import { Badge }  from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { PlateQueryDto } from '@/interfaces';
import { formatCurrency, formatDate, getStatusColor } from '@/lib/utils';

interface QueryResultProps {
  result: PlateQueryDto;
}

export function QueryResult({ result }: QueryResultProps) {
  return (
    <div className="space-y-4">
      {/* Resumen */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Placa {result.plate}</CardTitle>
            <Badge className={getStatusColor(result.status)}>
              {result.status}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="text-center p-3 bg-muted rounded-lg">
              <p className="text-2xl font-bold">{result.finesCount}</p>
              <p className="text-sm text-muted-foreground">Multas</p>
            </div>
            <div className="text-center p-3 bg-muted rounded-lg">
              <p className="text-2xl font-bold">{result.summonsCount}</p>
              <p className="text-sm text-muted-foreground">Comparendos</p>
            </div>
            <div className="text-center p-3 bg-muted rounded-lg col-span-2">
              <p className="text-2xl font-bold text-destructive">
                {formatCurrency(result.totalAmount)}
              </p>
              <p className="text-sm text-muted-foreground">Total a pagar</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Detalle de multas */}
      {result.fines.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">
              Multas ({result.fines.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {result.fines.map((fine, i) => (
              <div
                key={i}
                className="border rounded-lg p-3 space-y-1"
              >
                <div className="flex justify-between items-start">
                  <span className="text-sm font-medium">
                    {fine.number || `Multa #${i + 1}`}
                  </span>
                  <span className="font-bold text-destructive">
                    {formatCurrency(fine.amount)}
                  </span>
                </div>
                <p className="text-xs text-muted-foreground">
                  {fine.description}
                </p>
                <div className="flex gap-2 text-xs text-muted-foreground">
                  <span>{fine.agency}</span>
                  {fine.infractionDate && (
                    <span>· {fine.infractionDate}</span>
                  )}
                  {fine.status && (
                    <Badge variant="outline" className="text-xs">
                      {fine.status}
                    </Badge>
                  )}
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Detalle de comparendos */}
      {result.summons.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">
              Comparendos ({result.summons.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {result.summons.map((summons, i) => (
              <div
                key={i}
                className="border rounded-lg p-3 space-y-1"
              >
                <div className="flex justify-between items-start">
                  <span className="text-sm font-medium">
                    {summons.number || `Comparendo #${i + 1}`}
                  </span>
                  <span className="font-bold text-destructive">
                    {formatCurrency(summons.amount)}
                  </span>
                </div>
                <p className="text-xs text-muted-foreground">
                  {summons.infraction}
                </p>
                <div className="flex gap-2 text-xs text-muted-foreground">
                  {summons.infractionDate && (
                    <span>{summons.infractionDate}</span>
                  )}
                  {summons.status && (
                    <Badge variant="outline" className="text-xs">
                      {summons.status}
                    </Badge>
                  )}
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Sin registros */}
      {result.status === 'SinMultas' && (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">
              No se encontraron multas ni comparendos para la placa{' '}
              <strong>{result.plate}</strong>.
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}