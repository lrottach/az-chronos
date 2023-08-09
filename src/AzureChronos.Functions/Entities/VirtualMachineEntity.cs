using System.Threading.Tasks;
using AzureChronos.Functions.Interfaces;
using AzureChronos.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureChronos.Functions.Entities;

[JsonObject(MemberSerialization.OptIn)]
public class VirtualMachineEntity : IVirtualMachineEntity
{
    private readonly ILogger _log;
    private readonly IAzureComputeService _azureComputeService;
    
    [JsonProperty("subscriptionId")]
    public string SubscriptionId { get; set; }
    
    [JsonProperty("resourceGroupName")]
    public string ResourceGroupName { get; set; }
    
    [JsonProperty("virtualMachineName")]
    public string VirtualMachineName { get; set; }
    
    [JsonProperty("scheduled")]
    public bool Scheduled { get; set; }
    
    public VirtualMachineEntity(ILogger log, IAzureComputeService azureComputeService)
    {
        _log = log;
        _azureComputeService = azureComputeService;
    }
    
    public Task InitializeEntityAsync(EntityInitializePayload payload)
    {
        SubscriptionId = payload.SubscriptionId;
        ResourceGroupName = payload.ResourceGroupName;
        VirtualMachineName = payload.VirtualMachineName;
        Scheduled = false;
        
        return Task.CompletedTask;
    }

    // Method to validate if a virtual machine is eligible to be scheduled
    public Task<bool> ValidateVirtualMachineEligibilityAsync()
    {
        throw new System.NotImplementedException();
    }

    [FunctionName(nameof(VirtualMachineEntity))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<VirtualMachineEntity>(log);

}