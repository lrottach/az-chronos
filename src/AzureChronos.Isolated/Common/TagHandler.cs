using Azure.ResourceManager.Compute;

namespace AzureChronos.Isolated.Common;

/// <summary>
/// Represents a helper for handling Azure Resource Manager (ARM) tags.
/// </summary>
public static class TagHandler
{
    /// <summary>
    /// Checks if a specified tag exists on a given Azure Virtual Machine resource.
    /// </summary>
    /// <param name="vm">The Azure Virtual Machine resource.</param>
    /// <param name="tagName">The name of the tag to check.</param>
    /// <returns>True if the tag exists; otherwise, false.</returns>
    public static bool ValidateVirtualMachineTag(VirtualMachineResource vm, string? tagName)
    {
        return tagName != null && vm.Data.Tags.ContainsKey(tagName);
    }

    public static bool HasExclusionTagAssigned(VirtualMachineResource vm, string? exclusionTag) =>
        ValidateVirtualMachineTag(vm, exclusionTag);
    
    
}
