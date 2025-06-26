using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.SharedKernel.Application.Abstractions;


public interface IModuleConfiguration
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void Configure(IApplicationBuilder app, IWebHostEnvironment env);
    string ModuleName { get; }
    string RoutePrefix { get; }
}