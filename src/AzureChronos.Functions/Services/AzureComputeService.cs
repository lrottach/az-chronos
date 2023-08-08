using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
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
    public DefaultAzureCredential AzureCredential { get; } = new();

    /// <summary>
    /// Asynchronous method <c>ListAzureVirtualMachines</c> retrieves and returns all the virtual machines under the provided Azure subscription.
    /// This makes use of Azure ARM client provided by the Azure SDK for .NET Core applications.
    /// </summary>
    /// <param name="subscriptionId">Azure subscription id used to fetch virtual machines.</param>
    /// <returns>A list of Azure virtual machine resources.</returns>
    public async Task<List<VirtualMachineResource>> ListAzureVirtualMachines(string subscriptionId)
    {
        var client = new ArmClient(AzureCredential);

        var subResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
        var subResource = client.GetSubscriptionResource(subResourceId);
        
        var virtualMachines = new List<VirtualMachineResource>();
        
        await foreach(var vm in subResource.GetVirtualMachinesAsync())
        {
            virtualMachines.Add(vm);
            Console.WriteLine($"Found Azure Virtual Machine: {vm.Data.Name}");
        }

        return virtualMachines;
    }
}