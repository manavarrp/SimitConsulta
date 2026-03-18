# SimitConsulta — Consulta de Multas Vehiculares SIMIT

Sistema de consulta de multas y comparendos del SIMIT (Federación Colombiana de Municipios) con arquitectura Clean Architecture en .NET 8 y frontend Next.js.

---

## Requisitos previos

Antes de ejecutar el proyecto asegúrate de tener instalado:

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download |
| Node.js | 18.0 | https://nodejs.org |
| Docker Desktop | 4.0 | https://www.docker.com/products/docker-desktop |
| Git | cualquier | https://git-scm.com |

---

## Estructura del monorepo

```
SimitConsulta/
  backend/
    SimitConsulta.Domain/
    SimitConsulta.Application/
    SimitConsulta.Infrastructure/
    SimitConsulta.API/
    tests/
      SimitConsulta.UnitTests/
      SimitConsulta.IntegrationTests/
    backend.sln
  frontend/
    src/
      app/
      axios/
      components/
      hooks/
      interfaces/
      lib/
      providers/
      service/
      schemas/
      store/
  docker-compose.yml
  .env.example
  README.md
```

---

## Configuración inicial — paso a paso

### Paso 1 — Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd SimitConsulta
```

### Paso 2 — Configurar variables de entorno

Copia el archivo de ejemplo y edítalo con tu contraseña:

```bash
cp .env.example .env
```

Abre `.env` y establece la contraseña de SQL Server:

```env
SA_PASSWORD=TuPasswordSeguro2024!
DB_NAME=SimitConsultaDB
ASPNETCORE_ENVIRONMENT=Development
```

La contraseña debe cumplir los requisitos de SQL Server: mínimo 8 caracteres, mayúsculas, minúsculas, números y símbolos.

### Paso 3 — Levantar la base de datos con Docker

```bash
# Desde la raíz del monorepo (donde está docker-compose.yml)
docker-compose up -d
```

Espera unos segundos y verifica que el contenedor esté saludable:

```bash
docker ps
# Debe mostrar: simit-db ... (healthy)
```

Verifica que la base de datos fue creada:

```bash
docker exec simit-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "TuPasswordSeguro2024!" -No \
  -Q "SELECT name FROM sys.databases WHERE name = 'SimitConsultaDB'"
```

### Paso 4 — Configurar el backend

Crea el archivo de configuración local con la cadena de conexión:

```bash
cd backend/SimitConsulta.API
```

Crea el archivo `appsettings.Development.json` con el siguiente contenido (reemplaza el password con el que pusiste en `.env`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SimitConsultaDB;User Id=SA;Password=TuPasswordSeguro2024!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=30"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "SimitConsulta": "Debug"
    }
  }
}
```

### Paso 5 — Ejecutar el backend

```bash
# Desde la carpeta backend/
cd backend
dotnet run --project SimitConsulta.API
```

Al arrancar verás:

```
info: Applying EF Core migrations (attempt 1/5)...
info: Migrations applied successfully.
info: Now listening on: http://localhost:XXXX
```

EF Core crea automáticamente las tablas en la base de datos. Anota el puerto donde escucha el API.

Verifica que el API responde:

```bash
curl http://localhost:<PUERTO>/api/v1/health
# {"status":"ok","timestamp":"...","version":"1.0"}
```

La documentación Swagger está disponible en:

```
http://localhost:<PUERTO>/swagger
```

### Paso 6 — Configurar el frontend

```bash
cd frontend
npm install
```

Crea el archivo `.env` del frontend con el puerto del API:

```env
NEXT_PUBLIC_API_URL=http://localhost:<PUERTO>/api/v1
```

Reemplaza `<PUERTO>` con el puerto donde corre el API (por ejemplo `5229`).

### Paso 7 — Ejecutar el frontend

```bash
npm run dev
```

Abre el navegador en:

```
http://localhost:3000
```

---

## Ejecutar los tests

```bash
cd backend
dotnet test
```

Resultado esperado:

```
Passed! - Failed: 0, Passed: 35+, Skipped: 0
```

---

## Uso de la aplicación

La aplicación tiene tres secciones en una sola página:

**Consulta Individual** — ingresa una placa en formato `ABC123` (carro) o `ABC12D` (moto) y presiona Consultar. El sistema resuelve el captcha automáticamente y consulta el SIMIT.

**Consulta Masiva** — ingresa múltiples placas separadas por coma, punto y coma o salto de línea. Máximo 100 placas por consulta.

**Historial** — muestra todas las consultas realizadas con paginación. Puedes filtrar por placa.

---

## Arquitectura del captcha

El SIMIT no tiene API pública documentada. El sistema usa ingeniería inversa del portal web:

1. El frontend llama directamente al servidor del captcha `qxcaptcha.fcm.org.co` desde el navegador del usuario (TLS real de Chrome).
2. Resuelve el Proof-of-Work: SHA256 con nonces primos hasta encontrar un hash que empiece con `0000`.
3. Envía el token resuelto al backend junto con la placa.
4. El backend llama al SIMIT con el token y persiste el resultado.

---

## Variables de entorno

| Variable | Descripción | Ejemplo |
|---|---|---|
| `SA_PASSWORD` | Contraseña del SA de SQL Server | `MiPassword2024!` |
| `DB_NAME` | Nombre de la base de datos | `SimitConsultaDB` |
| `ASPNETCORE_ENVIRONMENT` | Entorno del API | `Development` |
| `NEXT_PUBLIC_API_URL` | URL base del backend | `http://localhost:5229/api/v1` |

---

## Solución de problemas frecuentes

**El contenedor Docker no arranca**
Verifica que Docker Desktop esté corriendo y que el puerto 1433 no esté ocupado por otra instancia de SQL Server.

**Error de conexión a la base de datos**
Verifica que el password en `appsettings.Development.json` coincide exactamente con `SA_PASSWORD` en el `.env`.

**El frontend no conecta al backend**
Verifica que `NEXT_PUBLIC_API_URL` en `frontend/.env` tiene el puerto correcto. Después de cambiar el `.env` debes reiniciar `npm run dev`.

**Error 401 del SIMIT al consultar**
El servidor del captcha puede rechazar tokens en ciertos momentos. Intenta de nuevo — el captcha se resuelve con cada consulta.

---

## Stack tecnológico

**Backend:** .NET 8, ASP.NET Core, Entity Framework Core 8, MediatR, FluentValidation, SQL Server

**Frontend:** Next.js 14, TypeScript, shadcn/ui, Zustand, React Hook Form, Zod, Axios, js-sha256

**Infraestructura:** Docker, SQL Server 2022

**Tests:** xUnit, Moq, FluentValidation.TestHelper, WebApplicationFactory
