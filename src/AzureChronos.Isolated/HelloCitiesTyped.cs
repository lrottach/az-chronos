using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AzureChronos.Isolated;

public class HelloCitiesTypedStarter
{
    [Function(nameof(StartHelloCitiesTyped))]
    public static async Task<HttpResponseData> StartHelloCitiesTyped(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient taskClient,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(StartHelloCitiesTyped));

        // Source generators are used to generate type-safe extension methods for scheduling class-based
        // orchestrators that are defined in the current project. The name of the generated extension methods
        // are based on the names of the orchestrator classes. Note that the source generator will *not*
        // generate type-safe extension methods for non-class-based orchestrator functions.
        // NOTE: This feature is in PREVIEW and requires a package reference to Microsoft.DurableTask.Generators.
        string instanceId = await taskClient.ScheduleNewHelloCitiesTypedInstanceAsync();
        logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);
        return taskClient.CreateCheckStatusResponse(req, instanceId);
    }
}

[DurableTask(nameof(HelloCitiesTyped))]
public class HelloCitiesTyped : TaskOrchestrator<string?, string>
{
    public async override Task<string> RunAsync(TaskOrchestrationContext context, string? input)
    {
        // Source generators are used to generate the type-safe activity function
        // call extension methods on the context object. The names of these generated
        // methods are derived from the names of the activity classes. Note that both
        // activity classes and activity functions are supported by the source generator.
        string result = "";
        result += await context.CallSayHelloTypedAsync("Tokyo") + " ";
        result += await context.CallSayHelloTypedAsync("London") + " ";
        result += await context.CallSayHelloTypedAsync("Seattle");
        return result;
    }
}

[DurableTask(nameof(SayHelloTyped))]
public class SayHelloTyped : TaskActivity<string, string>
{
    readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SayHelloTyped"/> class.
    /// This class is initialized once for every activity execution.
    /// </summary>
    /// <remarks>
    /// Activity class constructors support constructor-based dependency injection.
    /// The injected services are provided by the function's <see cref="FunctionContext.InstanceServices"/> property.
    /// </remarks>
    /// <param name="logger">The logger injected by the Azure Functions runtime.</param>
    public SayHelloTyped(ILogger<SayHelloTyped> logger)
    {
        this.logger = logger;
    }

    public override Task<string> RunAsync(TaskActivityContext context, string cityName)
    {
        this.logger.LogInformation("Saying hello to {name}", cityName);
        return Task.FromResult($"Hello, {cityName}!");
    }
}