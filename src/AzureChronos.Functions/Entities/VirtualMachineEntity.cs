using System.Threading.Tasks;
using AzureChronos.Functions.Common;
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
    public async Task<bool> ValidateVirtualMachineEligibilityAsync()
    {
        // Query the Azure API to get the virtual machine resource
        var virtualMachine = await _azureComputeService.GetAzureVirtualMachineAsync(SubscriptionId, ResourceGroupName, VirtualMachineName);
        return TagHandler.ValidateVirtualMachineTag(virtualMachine, "AzChronos_Startup");
    }

    [FunctionName(nameof(VirtualMachineEntity))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<VirtualMachineEntity>(log);

}