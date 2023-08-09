using System.Threading.Tasks;
using AzureChronos.Functions.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Azure.ResourceManager.Compute;

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
        log.LogInformation("[Orchestrator] Starting Orchestrator");
        var subId = Environment.GetEnvironmentVariable("AZ_SUBSCRIPTION_ID");
        Console.WriteLine($"[Orchestrator] Subscription Id: {subId}");
        var vmList = await _azureComputeService.ListAzureVirtualMachines(subId);

        var vmOutputs = new List<VirtualMachineResource>();
        
        foreach (var vm in vmList)
        {
            // TODO: If the validation activity function returns true leave the vm in the list, otherwise remove it
            vmOutputs.Add(
                await context.CallActivityAsync<VirtualMachineResource>("AzureChronosActivityValidation", vm));
        }
    }
}