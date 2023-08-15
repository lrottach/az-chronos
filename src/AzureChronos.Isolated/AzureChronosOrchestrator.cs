using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Isolated;

public class AzureChronosOrchestrator
{
    private readonly ILogger _logger;

    public AzureChronosOrchestrator(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AzureChronosOrchestrator>();
    }
    
    [Function(nameof(StartAzureChronosOrchestration))]
    public async Task StartAzureChronosOrchestration(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        _logger.LogInformation($"[OrchestrationTrigger] Starting Azure Chronos Orchestration");     
        await context.CallActivityAsync(nameof(AzureChronosActivities.ListAzureVirtualMachinesAsync), "1234567890");
    }
}