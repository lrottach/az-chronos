using AzureChronos.Isolated.Interfaces;
using AzureChronos.Isolated.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IAzureComputeService, AzureComputeService>();
    })
    .Build();

host.Run();