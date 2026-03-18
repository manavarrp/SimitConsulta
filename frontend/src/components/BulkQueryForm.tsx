'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button }   from '@/components/ui/button';
import { Label }    from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge }    from '@/components/ui/badge';
import { bulkQuerySchema, parsePlates, validatePlates } from '@/schemas';
import type { BulkQueryForm as BulkQueryFormType } from '@/schemas'; // ← type-only import
import { useBulkQuery } from '@/hooks/useBulkQuery';
import { List, Loader2 } from 'lucide-react';

export function BulkQueryForm() {
  const { bulkQuery, bulkLoading, bulkResult } = useBulkQuery();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<BulkQueryFormType>({
    resolver: zodResolver(bulkQuerySchema),
  });

  const onSubmit = async (data: BulkQueryFormType) => {
    // Parsear y validar el string de placas
    const plates = parsePlates(data.plates);
    const error  = validatePlates(plates);

    if (error) {
      setError('plates', { message: error });
      return;
    }

    await bulkQuery(plates);
  };

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <List className="w-5 h-5" />
            Consulta masiva
          </CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="plates">
                Placas (separadas por coma, punto y coma o salto de línea)
              </Label>
              <Textarea
                id="plates"
                placeholder={"ABC123\nXYZ986\nLMN45D"}
                rows={5}
                {...register('plates')}
                className="uppercase font-mono"
              />
              {errors.plates && (
                <p className="text-sm text-destructive">
                  {errors.plates.message}
                </p>
              )}
              <p className="text-xs text-muted-foreground">
                Máximo 100 placas por consulta.
              </p>
            </div>
            <Button
              type="submit"
              disabled={bulkLoading}
              className="w-full"
            >
              {bulkLoading ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Procesando lote...
                </>
              ) : (
                'Consultar lote'
              )}
            </Button>
          </form>
        </CardContent>
      </Card>

      {/* Resultados del lote — igual que antes */}
      {bulkResult && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">
                Resultado del lote
              </CardTitle>
              <div className="flex gap-2">
                <Badge variant="default">
                  {bulkResult.successful} exitosas
                </Badge>
                {bulkResult.failed > 0 && (
                  <Badge variant="destructive">
                    {bulkResult.failed} fallidas
                  </Badge>
                )}
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="space-y-2 max-h-96 overflow-y-auto">
              {bulkResult.results.map((r, i) => (
                <div
                  key={i}
                  className="flex items-center justify-between p-2 border rounded"
                >
                  <div className="flex items-center gap-2">
                    <span className="font-mono font-medium">
                      {r.plate || `Placa ${i + 1}`}
                    </span>
                    <Badge
                      variant={
                        r.status === 'Exitoso'   ? 'default'     :
                        r.status === 'SinMultas' ? 'secondary'   :
                        'destructive'
                      }
                      className="text-xs"
                    >
                      {r.status}
                    </Badge>
                  </div>
                  <div className="text-right text-sm">
                    {r.status === 'Exitoso' && (
                      <span className="text-destructive font-medium">
                        ${r.totalAmount.toLocaleString('es-CO')}
                      </span>
                    )}
                    {r.errorMessage && (
                      <span className="text-destructive text-xs">
                        {r.errorMessage}
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}