using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Functions;

public class AzureChronosTimerTrigger
{
    [FunctionName("AzureChronosTimerTrigger")]
    public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        string instanceId = await starter.StartNewAsync("AzureChronosOrchestrator", null); 
        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
    }
}