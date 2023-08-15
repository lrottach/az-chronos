using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Isolated;

public class AzureChronosActivities
{
    private readonly ILogger _logger;
    
    public AzureChronosActivities(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AzureChronosActivities>();
    }
    
    [Function(nameof(ListAzureVirtualMachinesAsync))]
    public async Task ListAzureVirtualMachinesAsync([ActivityTrigger] string subscriptionId, FunctionContext context)
    {
        _logger.LogInformation($"[Activity] Query Azure Virtual Machines for subscription {subscriptionId}", subscriptionId);
    }
    
}