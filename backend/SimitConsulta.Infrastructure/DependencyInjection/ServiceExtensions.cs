using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimitConsulta.Application.Common.Behaviors;
using SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.Queries.GetHistory;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Infrastructure.Persistence;
using SimitConsulta.Infrastructure.Persistence.Repositories;
using SimitConsulta.Infrastructure.Simit.Captcha;
using SimitConsulta.Infrastructure.Simit.Client;
using System.Net;

namespace SimitConsulta.Infrastructure.DependencyInjection;

/// <summary>
/// Punto único de registro de todos los servicios.
/// Program.cs llama solo AddInfrastructure() — no conoce los detalles.
///
/// Pipeline de MediatR (orden importa):
/// 1. LoggingBehavior  → mide tiempo total
/// 2. ValidationBehavior → valida antes del handler
/// 3. Handler          → lógica del caso de uso
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registra todos los servicios de Infrastructure y Application.
    /// Único método público de esta clase.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDatabase(config);
        services.AddSimitClient();
        services.AddApplicationServices();
        return services;
    }

    /// <summary>
    /// DbContext con SQL Server y repositorio.
    /// ConnectionString desde appsettings.json.
    /// </summary>
    private static void AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IPlateQueryRepository, PlateQueryRepository>();
    }

    /// <summary>
    /// HttpClient nombrado "simit" con headers requeridos por el SIMIT.
    /// Sin Origin y Referer el servidor rechaza la petición.
    /// </summary>
    private static void AddSimitClient(
     this IServiceCollection services)
    {
        services.AddHttpClient("simit", client =>
        {
            // Headers que SÍ funcionan — verificados con Invoke-WebRequest
            client.DefaultRequestHeaders.Add(
                "Origin", "https://www.fcm.org.co");
            client.DefaultRequestHeaders.Add(
                "Referer", "https://www.fcm.org.co/");
            client.DefaultRequestHeaders.Add(
                "Accept", "*/*");
            client.DefaultRequestHeaders.Add(
                "Accept-Language", "es-ES,es;q=0.9");
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/146.0.0.0 Safari/537.36");

            // ← Eliminados: Sec-Fetch-Dest, Sec-Fetch-Mode, Sec-Fetch-Site
            // ← Eliminados: sec-ch-ua, sec-ch-ua-mobile, sec-ch-ua-platform
            // Esos headers hacen que el servidor bloquee la conexión

            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseProxy = true,
            Proxy = WebRequest.GetSystemWebProxy(),
            UseDefaultCredentials = true,
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        services.AddScoped<ICaptchaSolver, CaptchaSolver>();
        services.AddScoped<ISimitGateway, SimitHttpClient>();
    }
    /// <summary>
    /// MediatR, pipeline behaviors en orden y FluentValidation.
    /// </summary>
    private static void AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(
                typeof(QueryPlateCommand).Assembly,
                typeof(BulkQueryCommand).Assembly,
                typeof(GetHistoryQuery).Assembly));

        // Orden de registro = orden de ejecución en el pipeline
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(
            typeof(QueryPlateValidator).Assembly);
    }
}