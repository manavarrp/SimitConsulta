'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button }   from '@/components/ui/button';
import { Input }    from '@/components/ui/input';
import { Label }    from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { queryPlateSchema, QueryPlateForm } from '@/schemas';
import { useQueryPlate } from '@/hooks/useQueryPlate';
import { Search, Loader2 } from 'lucide-react';

export function PlateQueryForm() {
  const { queryPlate, queryLoading } = useQueryPlate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<QueryPlateForm>({
    resolver: zodResolver(queryPlateSchema),
  });

  const onSubmit = async (data: QueryPlateForm) => {
    await queryPlate(data.plate);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Search className="w-5 h-5" />
          Consulta individual
        </CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="plate">Placa vehicular</Label>
            <Input
              id="plate"
              placeholder="ABC123"
              maxLength={6}
              {...register('plate')}
              className="uppercase"
            />
            {errors.plate && (
              <p className="text-sm text-destructive">
                {errors.plate.message}
              </p>
            )}
          </div>
          <Button
            type="submit"
            disabled={queryLoading}
            className="w-full"
          >
            {queryLoading ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Consultando...
              </>
            ) : (
              'Consultar SIMIT'
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}