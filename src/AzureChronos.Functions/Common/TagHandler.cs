using Azure.ResourceManager.Compute;

namespace AzureChronos.Functions.Common;

public static class TagHandler
{
    public static bool ValidateVirtualMachineTag(VirtualMachineResource vm, string tagName)
    {
        return vm.Data.Tags.ContainsKey(tagName);
    }
}