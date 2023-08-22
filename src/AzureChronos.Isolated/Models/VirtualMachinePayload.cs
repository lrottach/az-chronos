using Azure.ResourceManager.Compute;
using AzureChronos.Isolated.Enumerators;

namespace AzureChronos.Isolated.Models;

// Class to hold the VirtualMachineResource itself and all required data like schedules and tags
public class VirtualMachinePayload
{
    public VirtualMachinePayload(VirtualMachineResource virtualMachineResource)
    {
        VirtualMachineData = virtualMachineResource;

        DeallocateCron = TryGetCronSchedule(Environment.GetEnvironmentVariable("AZ_CHRONOS_TAG_DEALLOCATE"));
        StartupCron = TryGetCronSchedule(Environment.GetEnvironmentVariable("AZ_CHRONOS_TAG_STARTUP"));
    }

    private VirtualMachineResource VirtualMachineData { get; set; }
    
    public  string DeallocateCron { get; set; }
    
    public string StartupCron { get; set; }

    public DateTime? NextAction { get; set; }

    public AzureChronosActions NextActionType { get; set; }

    private string TryGetCronSchedule(string? tagName)
    {
        return tagName != null && VirtualMachineData.Data.Tags.TryGetValue(tagName, out var cron) 
            ? cron 
            : string.Empty;
    }

    public void CalculateAzureChronosAction()
    {
         
    }
}