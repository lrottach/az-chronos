using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureChronos.Functions.Common;
using AzureChronos.Functions.Enumerators;
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
    
    // Azure Chronos Tag names
    private const string AzureChronosExclusionTag = "AzChronos_Exclude";
    private const string AzureChronosDeallocateTag = "AzChronos_Deallocate";
    private const string AzureChronosStartupTag = "AzChronos_Startup";

[JsonProperty("subscriptionId")]
    public string SubscriptionId { get; set; }
    
    [JsonProperty("resourceGroupName")]
    public string ResourceGroupName { get; set; }
    
    [JsonProperty("virtualMachineName")]
    public string VirtualMachineName { get; set; }
    
    [JsonProperty("scheduled")]
    public bool Scheduled { get; set; }
    
    [JsonProperty("schedules")]
    public List<AzureChronosSchedule> Schedules { get; set; }
    
    public VirtualMachineEntity(ILogger log, IAzureComputeService azureComputeService)
    {
        _log = log;
        _azureComputeService = azureComputeService;
    }
    
    public async Task InitializeEntityAsync(EntityInitializePayload payload)
    {
        // Initialize the entity properties
        // Set scheduled to false by default
        SubscriptionId = payload.SubscriptionId;
        ResourceGroupName = payload.ResourceGroupName;
        VirtualMachineName = payload.VirtualMachineName;
        Scheduled = false;

        // Validate if the virtual machine is eligible to be scheduled
        // If not, stop here and delete the entity
        // If ValidateVirtualMachineEligibilityAsync() returns false, the entity will be deleted
        if (!await ValidateVirtualMachineEligibilityAsync())
        {
            _log.LogWarning($"[Entity] Virtual machine {VirtualMachineName} is not eligible to be scheduled.");
            // Delete the entity if the virtual machine is not eligible to be scheduled
            DeleteEntity();
        }
    }

    /// <summary>
    /// Asynchronously validates if a virtual machine is eligible for Azure Chronos scheduling.
    /// </summary>
    /// <returns>
    /// A Task representing the asynchronous operation, containing a boolean value indicating the eligibility.
    /// - Returns true if the virtual machine has one of the required deallocate or startup tags.
    /// - Returns false if the virtual machine is tagged with an exclusion tag or does not have the required tags.
    /// </returns>
    public async Task<bool> ValidateVirtualMachineEligibilityAsync()
    {
        // Query the Azure API to get the virtual machine resource
        var virtualMachine = await _azureComputeService.GetAzureVirtualMachineAsync(SubscriptionId, ResourceGroupName, VirtualMachineName);

        // Exit directly if the exclusion tag is present
        if (TagHandler.ValidateVirtualMachineTag(virtualMachine, AzureChronosExclusionTag))
        {
            _log.LogInformation($"[Entity] Virtual machine {VirtualMachineName} is excluded from scheduling.");
            return false;
        }
        
        // Check if the virtual machine has one of the required deallocate or startup tags
        var validTags = new HashSet<string> {AzureChronosDeallocateTag, AzureChronosStartupTag};
        foreach (var tag in validTags)
        {
            if (TagHandler.ValidateVirtualMachineTag(virtualMachine, tag))
            {
                return true;
            }
        }
        
        return false;
    }

    public async Task AddVirtualMachineSchedules()
    {
        // Query the Azure API to get the virtual machine resource
        var virtualMachine = await _azureComputeService.GetAzureVirtualMachineAsync(SubscriptionId, ResourceGroupName, VirtualMachineName);
        
        // Check if the virtual machine has one of the required deallocate or startup tags
        TryAddScheduleFromTag(virtualMachine.Data.Tags, AzureChronosDeallocateTag, AzureChronosActions.Deallocate);
        TryAddScheduleFromTag(virtualMachine.Data.Tags, AzureChronosStartupTag, AzureChronosActions.Start);
    }
    
    private void TryAddScheduleFromTag(IDictionary<string, string> tags, string tagName, AzureChronosActions action)
    {
        if (tags.TryGetValue(tagName, out var cron))
        {
            var scheduleTime = CronHandler.GetNextOccurrence(cron);
            if (scheduleTime.HasValue)
            {
                Schedules.Add(new AzureChronosSchedule
                {
                    Schedule = scheduleTime.Value,
                    CronExpression = cron,
                    Action = action,
                });
            }
        }
    }
    
    public void DeleteEntity()
    {
        _log.LogInformation($"[Entity] Deleting entity {VirtualMachineName}.");
        Entity.Current.DeleteState();
    }

    [FunctionName(nameof(VirtualMachineEntity))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        => ctx.DispatchAsync<VirtualMachineEntity>(log);

}