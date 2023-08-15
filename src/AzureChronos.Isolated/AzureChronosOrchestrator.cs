using Azure.ResourceManager.Compute;
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
        // Gather required environment variables
        var subscriptionId = Environment.GetEnvironmentVariable("AZ_SUBSCRIPTION_ID");
        
        _logger.LogInformation($"[OrchestrationTrigger] Starting Azure Chronos Orchestration");
        var vms = await context.CallActivityAsync<List<VirtualMachineResource>>(
            nameof(AzureChronosActivities.ListAzureVirtualMachinesAsync), subscriptionId);
    }
}