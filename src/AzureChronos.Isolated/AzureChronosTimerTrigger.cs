using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Isolated;

public class AzureChronosTimerTrigger
{
    private readonly ILogger _logger;

    public AzureChronosTimerTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AzureChronosTimerTrigger>();
    }
    
    [Function(nameof(StartAzureChronos))]
    public void StartAzureChronos(
        [TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient client,
        FunctionContext context)
    {
        _logger.LogInformation($"[TimerTrigger] Starting Azure Chronos");
    }
}
