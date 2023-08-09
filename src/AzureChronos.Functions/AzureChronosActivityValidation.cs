using Azure.ResourceManager.Compute;
using AzureChronos.Functions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Functions;

public class AzureChronosActivityValidation
{
    [FunctionName("AzureChronosActivityValidation")]
    public VirtualMachineResource RunActivityMethod([ActivityTrigger] VirtualMachineResource vm, ILogger log)
    {
        // TODO: Rewrite method to run validation and return true or false
        log.LogInformation($"[ActivityFunction] Validating Azure Virtual Machine: {vm.Data.Name}");

        // Validate if the VM has a tag named "Chronos" with value "True" assigned
        return TagHandler.ValidateVirtualMachineTag(vm,"AzureChronos_Schedule") ? vm : null;
    }
}