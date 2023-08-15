using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using AzureChronos.Isolated;

namespace AzureChronos.Isolated;

public class AzureChronosTimerTrigger
{
    private readonly ILogger _logger;

    public AzureChronosTimerTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AzureChronosTimerTrigger>();
    }
    
    [Function(nameof(StartAzureChronos))]
    public async Task StartAzureChronos(
        [TimerTrigger("0 */30 * * * *")] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient client,
        FunctionContext context)
    {
        _logger.LogInformation($"[TimerTrigger] Starting Azure Chronos");
        var instanceId =
            await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(AzureChronosOrchestrator.StartAzureChronosOrchestration));
        _logger.LogInformation($"[TimerTrigger] Started Azure Chronos with instance ID = {instanceId}", instanceId);
    }
}
