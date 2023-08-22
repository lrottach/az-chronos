using Azure.ResourceManager.Compute;
using AzureChronos.Isolated.Common;
using AzureChronos.Isolated.Interfaces;
using AzureChronos.Isolated.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Isolated;

public class AzureChronosActivities
{
    private readonly ILogger _logger;
    private readonly IAzureComputeService _azureComputeService;
    private readonly string? _azureChronosExclusionTag;
    private readonly string? _azureChronosDeallocateTag;
    private readonly string? _azureChronosStartupTag;

    public AzureChronosActivities(ILoggerFactory loggerFactory, IAzureComputeService azureComputeService)
    {
        _logger = loggerFactory.CreateLogger<AzureChronosActivities>();
        _azureComputeService = azureComputeService;

        // Get required Azure Chronos Tags from environment variables
        _azureChronosExclusionTag = Environment.GetEnvironmentVariable("AzureChronosExclusionTag");
        _azureChronosDeallocateTag = Environment.GetEnvironmentVariable("AzureChronosDeallocateTag");
        _azureChronosStartupTag = Environment.GetEnvironmentVariable("AzureChronosStartupTag");
    }

    [Function(nameof(ListAzureVirtualMachinesAsync))]
    public async Task<List<VirtualMachineResource>> ListAzureVirtualMachinesAsync([ActivityTrigger] string subscriptionId, FunctionContext context)
    {
        _logger.LogInformation($"[Activity] Query Azure Virtual Machines for subscription {subscriptionId}");
        var vmResult = await _azureComputeService.ListAzureVirtualMachinesAsync(subscriptionId);

        if (vmResult.Count == 0)
        {
            _logger.LogInformation($"[Activity] No Azure Virtual Machines found for subscription {subscriptionId}");
        }
        else
        {
            _logger.LogInformation($"[Activity] Found {vmResult.Count} Azure Virtual Machines for subscription {subscriptionId}");

            foreach (var vm in vmResult)
            {
                _logger.LogInformation($"[Activity] Returning Azure Virtual Machine: {vm.Id}");
            }
        }

        return vmResult;
    }

    /// <summary>
    /// Validates the eligibility of a list of Azure Virtual Machines for scheduling based on their assigned tags.
    /// </summary>
    /// <param name="vms">The list of Azure Virtual Machines to validate.</param>
    /// <param name="context">The function execution context.</param>
    /// <returns>A list of <see cref="VirtualMachinePayload"/> objects representing the eligible virtual machines.</returns>
    [Function(nameof(CheckVirtualMachineCompatibilityAsync))]
    public Task<List<VirtualMachinePayload>> CheckVirtualMachineCompatibilityAsync([ActivityTrigger] List<VirtualMachineResource> vms, FunctionContext context)
    {
        _logger.LogInformation($"[Activity] Validate Azure Virtual Machine eligibility");

        var validVmsList = vms
            .Where(IsEligibleForScheduling)
            .Select(vm => new VirtualMachinePayload(vm))
            .ToList();

        return Task.FromResult(validVmsList);
    }

    /// <summary>
    /// Determines whether a given Azure Virtual Machine is eligible for scheduling based on its assigned tags.
    /// </summary>
    /// <param name="vm">The Azure Virtual Machine to validate.</param>
    /// <returns>True if the virtual machine is eligible for scheduling, false otherwise.</returns>
    private bool IsEligibleForScheduling(VirtualMachineResource vm)
    {
        if (HasExclusionTagAssigned(vm, _azureChronosExclusionTag))
        {
            // If the virtual machine has the exclusion tag, it is excluded from scheduling
            _logger.LogInformation($"[Activity] Azure Virtual Machine {vm.Id} is excluded from scheduling");
            return false;
        }

        if (!HasRequiredTagsAssigned(vm))
        {
            // If the virtual machine does not have one of the required tags, it is not eligible for scheduling
            _logger.LogInformation($"[Activity] Azure Virtual Machine {vm.Id} is not eligible for scheduling");
            return false;
        }

        // Assuming the virtual machine has at least one of the required tags, it is eligible for scheduling
        _logger.LogInformation($"[Activity] Azure Virtual Machine {vm.Id} is eligible for scheduling");
        return true;
    }

    private bool HasExclusionTagAssigned(VirtualMachineResource vm, string? exclusionTag) =>
        TagHandler.ValidateVirtualMachineTag(vm, exclusionTag);

    private bool HasRequiredTagsAssigned(VirtualMachineResource vm) =>
        TagHandler.ValidateVirtualMachineTag(vm, _azureChronosDeallocateTag) ||
        TagHandler.ValidateVirtualMachineTag(vm, _azureChronosStartupTag);

}