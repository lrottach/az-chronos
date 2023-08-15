using Microsoft.DurableTask;

namespace AzureChronos.Isolated;

[DurableTask(nameof(AzureChronosOrchestrator))]
public class AzureChronosOrchestrator : TaskOrchestrator<string?, string>
{
    public override Task<string> RunAsync(TaskOrchestrationContext context, string? input)
    {
        Console.WriteLine($"Input: {input}");
        return Task.FromResult("");
    }
}