using System.Threading.Tasks;
using AzureChronos.Functions.Models;

namespace AzureChronos.Functions.Interfaces;

public interface IVirtualMachineEntity
{
    public Task InitializeEntityAsync(EntityInitializePayload payload);
    public Task<bool> ValidateVirtualMachineEligibilityAsync();
}