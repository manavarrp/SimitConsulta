import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import { ToastProvider } from '@/providers/ToastProvider';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title:       'SimitConsulta — Consulta de multas vehiculares',
  description: 'Consulta multas y comparendos del SIMIT (FCM Colombia)',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="es">
      <body className={inter.className}>
        {children}
        <ToastProvider />
      </body>
    </html>
  );
}