using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using AzureChronos.Functions.Interfaces;

namespace AzureChronos.Functions.Services;

/// <summary>
/// Class <c>AzureComputeService</c> provides Azure related compute operations.
/// It implements the <c>IAzureComputeService</c> interface.
/// </summary>
public class AzureComputeService : IAzureComputeService
{
    /// <summary>
    /// Property <c>AzureCredential</c> takes care of providing credentials for Azure SDK usage. 
    /// It uses DefaultAzureCredential mechanism.
    /// </summary>
    private DefaultAzureCredential AzureCredential { get; } = new();
    
    /// <summary>
    /// Asynchronous method <c>ListAzureVirtualMachines</c> retrieves and returns all the virtual machines under the provided Azure subscription.
    /// This makes use of Azure ARM client provided by the Azure SDK for .NET Core applications.
    /// </summary>
    /// <param name="subscriptionId">Azure subscription id used to fetch virtual machines.</param>
    /// <returns>A list of Azure virtual machine resources.</returns>
    public async Task<List<VirtualMachineResource>> ListAzureVirtualMachinesAsync(string subscriptionId)
    {
        var client = new ArmClient(AzureCredential);

        var subResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
        var subResource = client.GetSubscriptionResource(subResourceId);
        
        var virtualMachines = new List<VirtualMachineResource>();
        
        await foreach(var vm in subResource.GetVirtualMachinesAsync())
        {
            virtualMachines.Add(vm);
        }

        return virtualMachines;
    }

    public async Task<VirtualMachineResource> GetAzureVirtualMachineAsync(string subscriptionId, string resourceGroupName, string vmName)
    {
        var client = new ArmClient(AzureCredential);
        
        var resourceId = VirtualMachineResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, vmName);
        var virtualMachine = client.GetVirtualMachineResource(resourceId);
        
        // Invoke the GetAsync() method to retrieve the virtual machine resource
        var expand = InstanceViewType.UserData;
        return await virtualMachine.GetAsync(expand: expand);
    }
}