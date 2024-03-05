using Azure.ResourceManager.Compute;

namespace AzureChronos.Functions.Common;

public static class TagHandler
{
    public static bool ValidateVirtualMachineTag(VirtualMachineResource vm, string tagName)
    {
        return vm.Data.Tags.ContainsKey(tagName);
    }
    
    public static bool IsVmExcludedForScheduling(VirtualMachineResource vm, string exclusionTagName)
    {
        return vm.Data.Tags.ContainsKey(exclusionTagName);
    }
    
    public static bool IsVmValidForScheduling(VirtualMachineResource vm, string tagName)
    {
        return vm.Data.Tags.ContainsKey(tagName);
    }
}