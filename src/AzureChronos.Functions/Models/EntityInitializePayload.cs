namespace AzureChronos.Functions.Models;

public class EntityInitializePayload
{
    public string SubscriptionId { get; set; }
    public string ResourceGroupName { get; set; }
    public string VirtualMachineName { get; set; }
}