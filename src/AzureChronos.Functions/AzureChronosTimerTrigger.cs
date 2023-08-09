using System;
using System.Threading.Tasks;
using AzureChronos.Functions.Common;
using AzureChronos.Functions.Interfaces;
using AzureChronos.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Functions;

public class AzureChronosTimerTrigger
{
    private readonly IAzureComputeService _azureComputeService;

    public AzureChronosTimerTrigger(IAzureComputeService azureComputeService)
    {
        _azureComputeService = azureComputeService;
    }
    
    [FunctionName("AzureChronosTimerTrigger")]
    public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
        [DurableClient] IDurableClient client,
        ILogger log)
    {
        // Gather required environment variables
        var subscriptionId = Environment.GetEnvironmentVariable("AZ_SUBSCRIPTION_ID");
        
        log.LogInformation($"[TimerTrigger] C# Timer trigger function executed at: {DateTime.Now}");
        
        // Query a list of all virtual machines in the subscription
        var virtualMachines = await _azureComputeService.ListAzureVirtualMachines(subscriptionId);
        
        log.LogInformation("[TimerTrigger] Found {vmCount} virtual machines in subscription {subscriptionId}", virtualMachines.Count, subscriptionId);

        foreach (var vm in virtualMachines)
        {
            // Validate if an entity already exists and an event was already scheduled
            var entityId = new EntityId(nameof(VirtualMachineEntity), vm.Id.ToString().Replace("/", ""));
            var entityState = await client.ReadEntityStateAsync<VirtualMachineEntity>(entityId);

            // If the entity already exists and an event was already scheduled, skip
            if (entityState.EntityExists && entityState.EntityState.Scheduled)
            {
                log.LogWarning(
                    "[TimerTrigger] Entity for Azure Virtual Machine '{vmId}' already exists and Azure Chronos is already scheduled. Skipping...",
                    vm.Id);
                continue;
            }

            // Initialize the entity
            log.LogInformation("[TimerTrigger] Initializing entity for Azure Virtual Machine '{vmId}'", vm.Id);
            await client.SignalEntityAsync<IVirtualMachineEntity>(entityId, proxy => proxy.InitializeEntityAsync(
                new Models.EntityInitializePayload
                {
                    SubscriptionId = subscriptionId,
                    ResourceGroupName = StringHandler.ExtractResourceGroupName(vm.Id.ToString()),
                    VirtualMachineName = vm.Data.Name
                }));
        }
    }
}