using Common.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Infrastructure;

public class ReportsModule : IModuleConfiguration
{
    public string ModuleName => "Reports";
    public string RoutePrefix => "api/reports";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Servicios específicos de reportes
        // TODO: Agregar servicios cuando se definan los requerimientos
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
    }
}
