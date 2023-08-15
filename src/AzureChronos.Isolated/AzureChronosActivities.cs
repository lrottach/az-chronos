using Azure.ResourceManager.Compute;
using AzureChronos.Isolated.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Isolated;

public class AzureChronosActivities
{
    private readonly ILogger _logger;
    private readonly IAzureComputeService _azureComputeService;
    
    public AzureChronosActivities(ILoggerFactory loggerFactory, IAzureComputeService azureComputeService)
    {
        _logger = loggerFactory.CreateLogger<AzureChronosActivities>();
        _azureComputeService = azureComputeService;
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
    
}