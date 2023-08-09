using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.ResourceManager.Compute;

namespace AzureChronos.Functions.Interfaces;

public interface IAzureComputeService : IAzureService
{
    Task<List<VirtualMachineResource>> ListAzureVirtualMachinesAsync(string subscriptionId);
    Task<VirtualMachineResource> GetAzureVirtualMachineAsync(string subscriptionId, string resourceGroupName, string vmName);
}