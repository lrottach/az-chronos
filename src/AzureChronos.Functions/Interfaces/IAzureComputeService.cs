using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.ResourceManager.Compute;

namespace AzureChronos.Functions.Interfaces;

public interface IAzureComputeService : IAzureService
{
    Task<List<VirtualMachineResource>> ListAzureVirtualMachines(string subscriptionId);
}