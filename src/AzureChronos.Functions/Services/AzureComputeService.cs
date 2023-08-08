using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using AzureChronos.Functions.Interfaces;

namespace AzureChronos.Functions.Services;

public class AzureComputeService : IAzureComputeService
{
    public DefaultAzureCredential AzureCredential { get; } = new();

    // Function to get all Azure VMs in a subscription using Azure SDK for .NET
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