using System.Threading.Tasks;
using AzureChronos.Functions.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

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
        log.LogInformation("Starting AzureChronos_Orchestrator");
        // var vmList = await _azureComputeService.ListAzureVirtualMachines("");
    }
}