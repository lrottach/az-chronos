using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Functions;

public static class AzureChronosTimerTrigger
{
    [FunctionName("AzureChronos_TimerTrigger")]
    public static async Task RunTimerTrigger(
        [TimerTrigger("0 */30 * * * *")]
        [DurableClient] IDurableOrchestrationClient starter,
        TimerInfo timerInfo,
        ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
        string instanceId = await starter.StartNewAsync("HelloOrch", null); 
        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
    }
}