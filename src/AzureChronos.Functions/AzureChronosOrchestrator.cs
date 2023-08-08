using System.Threading.Tasks;
using AzureChronos.Functions.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;

namespace AzureChronos.Functions;

public class AzureChronosOrchestrator
{
    private readonly IAzureComputeService _azureComputeService;

    public AzureChronosOrchestrator(IAzureComputeService azureComputeService)
    {
        _azureComputeService = azureComputeService;
    }
    
    [FunctionName("AzureChronosOrchestrator")]
    public async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        log.LogInformation("Starting AzureChronosOrchestrator");
        var subId = Environment.GetEnvironmentVariable("AZ_SUBSCRIPTION_ID");
        Console.WriteLine($"Subscription ID: {subId}");
        var vmList = await _azureComputeService.ListAzureVirtualMachines(subId);
    }
}