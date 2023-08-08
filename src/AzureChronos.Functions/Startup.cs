using AzureChronos.Functions.Interfaces;
using AzureChronos.Functions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureChronos.Functions.Startup))]

namespace AzureChronos.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddTransient<IAzureComputeService, AzureComputeService>();
    }
}