'use client';

import { Toaster } from '@/components/ui/sonner';

export function ToastProvider() {
  return (
    <Toaster
      richColors
      position="top-right"
      toastOptions={{
        duration: 4000,
      }}
    />
  );
}