using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
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

    /// <summary>
    /// This is an asynchronous method that retrieves a Virtual Machine resource from Azure.
    /// </summary>
    /// <param name="subscriptionId">A string parameter that specifies the Azure subscription ID.</param>
    /// <param name="resourceGroupName">A string parameter that specifies the resource group's name in Azure.</param>
    /// <param name="vmName">A string parameter that specifies the name of the virtual machine in Azure.</param>
    /// <returns>Returns a Task resulting in the VirtualMachineResource object representing the Azure Virtual Machine resource.</returns>
    /// <remark>
    /// This method uses Microsoft.Azure.Management.Fluent to interact with Azure. 
    /// It first creates the ArmClient, which is then used to get the VirtualMachineResource using the provided subscriptionId, resourceGroupName, and vmName.
    /// It then retrieves the Virtual Machine resource by invoking the GetAsync() method.
    /// </remark>
    /// <example>
    /// <code>
    /// VirtualMachineResource vmResource = await GetAzureVirtualMachineAsync(subscriptionId, resourceGroup, vmName);
    /// </code>
    /// </example>
    public async Task<VirtualMachineResource> GetAzureVirtualMachineAsync(string subscriptionId, string resourceGroupName, string vmName)
    {
        var client = new ArmClient(AzureCredential);
        
        var resourceId = VirtualMachineResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, vmName);
        var virtualMachine = client.GetVirtualMachineResource(resourceId);
        
        // Invoke the GetAsync() method to retrieve the virtual machine resource
        var expand = InstanceViewType.UserData;
        return await virtualMachine.GetAsync(expand: expand);
    }

    /// <summary>
    /// Asynchronously deallocates a specified Azure Virtual Machine.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID for the Azure account.</param>
    /// <param name="resourceGroupName">The name of the resource group in which the virtual machine is located.</param>
    /// <param name="vmName">The name of the virtual machine to be deallocated.</param>
    /// <returns>A Task representing the asynchronous operation of deallocating the virtual machine.</returns>
    public async Task DeallocateAzureVirtualMachineAsync(string subscriptionId, string resourceGroupName, string vmName)
    {
        var client = new ArmClient(AzureCredential);
        var resourceId = VirtualMachineResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, vmName);
        var virtualMachine = client.GetVirtualMachineResource(resourceId);
        
        // Invoke the DeallocateAsync() method to deallocate the virtual machine
        await virtualMachine.DeallocateAsync(WaitUntil.Completed);
    }

    /// <summary>
    /// Asynchronously starts a specified Azure Virtual Machine.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID for the Azure account.</param>
    /// <param name="resourceGroupName">The name of the resource group in which the virtual machine is located.</param>
    /// <param name="vmName">The name of the virtual machine to be started.</param>
    /// <returns>A Task representing the asynchronous operation of starting the virtual machine.</returns>
    public async Task StartAzureVirtualMachineAsync(string subscriptionId, string resourceGroupName, string vmName)
    {
        var client = new ArmClient(AzureCredential);
        var resourceId = VirtualMachineResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, vmName);
        var virtualMachine = client.GetVirtualMachineResource(resourceId);
        
        // Invoke the StartAsync() method to start the virtual machine
        await virtualMachine.PowerOnAsync(WaitUntil.Completed);
    }
}