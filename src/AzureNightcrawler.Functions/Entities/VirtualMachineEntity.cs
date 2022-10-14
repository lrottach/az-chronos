using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace AzureNightcrawler.Functions.Entities;

[JsonObject(MemberSerialization.OptIn)]
public class VirtualMachineEntity
{
    [JsonProperty("isScheduled")]
    public bool IsScheduled { get; set; }
    
    [JsonProperty("isSessionHost")]
    public bool IsSessionHost { get; set; }

    public async Task InitializeEntityAsync()
    {
        return;
    }

    public async Task ScheduleEventAsync()
    {
        
    }

    private void DeleteEntity()
    {
        Entity.Current.DeleteState();
    }
    
}