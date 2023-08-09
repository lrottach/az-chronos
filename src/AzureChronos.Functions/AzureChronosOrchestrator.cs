using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Azure.ResourceManager.Compute;

namespace AzureChronos.Functions;

public class AzureChronosOrchestrator
{
    [FunctionName("AzureChronosOrchestrator")]
    public async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        var subId = Environment.GetEnvironmentVariable("AZ_SUBSCRIPTION_ID");
        
        log.LogInformation("[Orchestrator] Starting Orchestrator");
        log.LogInformation($"[Orchestrator] Processing Azure Virtual Machines for subscription Id: {subId}");
        
        // Calling activity function to list all Azure Virtual Machines
        var vmList = await context.CallActivityAsync<List<VirtualMachineResource>>("AzureChronosActivityListMachines", subId);
    }
}