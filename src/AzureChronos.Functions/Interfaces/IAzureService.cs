using Azure.Identity;

namespace AzureChronos.Functions.Interfaces;

public interface IAzureService
{
    DefaultAzureCredential AzureCredential { get; }
}