'use client';

import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { PlateQueryForm } from '@/components/PlateQueryForm';
import { BulkQueryForm }  from '@/components/BulkQueryForm';
import { HistoryTable }   from '@/components/HistoryTable';
import { QueryResult }    from '@/components/QueryResult';
import { useSimitStore }  from '@/store/simitStore';
import { Car }            from 'lucide-react';

export default function Home() {
  const { queryResult, queryError, activeTab, setActiveTab } = useSimitStore();
  console.log("queryResult:", queryResult);
    console.log("activeTab:", activeTab);


  return (
    <main className="min-h-screen bg-background">
      {/* Header */}
      <div className="border-b bg-card">
        <div className="container mx-auto px-4 py-4 flex items-center gap-3">
          <div className="p-2 bg-primary rounded-lg">
            <Car className="w-6 h-6 text-primary-foreground" />
          </div>
          <div>
            <h1 className="text-xl font-bold">SimitConsulta</h1>
            <p className="text-sm text-muted-foreground">
              Consulta de multas vehiculares — SIMIT Colombia
            </p>
          </div>
        </div>
      </div>

      {/* Contenido */}
      <div className="container mx-auto px-4 py-6 max-w-4xl">
        <Tabs
          value={activeTab}
          onValueChange={setActiveTab}
          className="space-y-6"
        >
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="individual">Individual</TabsTrigger>
            <TabsTrigger value="masiva">Masiva</TabsTrigger>
            <TabsTrigger value="historial">Historial</TabsTrigger>
          </TabsList>

          {/* Tab consulta individual */}
          <TabsContent value="individual" className="space-y-4">
            <PlateQueryForm />
            {queryResult && activeTab === 'individual' && (
              <QueryResult result={queryResult} />
            )}
          </TabsContent>

          {/* Tab consulta masiva */}
          <TabsContent value="masiva">
            <BulkQueryForm />
          </TabsContent>

          {/* Tab historial */}
          <TabsContent value="historial">
            <HistoryTable />
          </TabsContent>
        </Tabs>
      </div>
    </main>
  );
}