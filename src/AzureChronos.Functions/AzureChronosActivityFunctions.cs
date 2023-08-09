using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.ResourceManager.Compute;
using AzureChronos.Functions.Common;
using AzureChronos.Functions.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Functions;

public class AzureChronosActivityFunctions
{
    private readonly IAzureComputeService _azureComputeService;

    public AzureChronosActivityFunctions(IAzureComputeService azureComputeService)
    {
        _azureComputeService = azureComputeService;
    }
    
    [FunctionName("AzureChronosActivityValidation")]
    public async Task<bool> RunVirtualMachineTagValidation([ActivityTrigger] VirtualMachineResource vm, ILogger log)
    {
        // TODO: Rewrite method to run validation and return true or false
        log.LogInformation($"[ActivityFunction] Validating Azure Virtual Machine: {vm.Data.Name}");

        // Validate if the VM has a tag named "Chronos" with value "True" assigned
        return TagHandler.ValidateVirtualMachineTag(vm,"AzureChronos_Schedule");
    }
    
    [FunctionName("AzureChronosActivityListMachines")]
    public async Task<List<VirtualMachineResource>> ListAzureVirtualMachines([ActivityTrigger] string subscriptionId, ILogger log)
    {
        log.LogInformation("[Activity] Querying available Azure Virtual Machines");
        return await _azureComputeService.ListAzureVirtualMachines(subscriptionId);
    }
}