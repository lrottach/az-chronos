using System;
using AzureChronos.Functions.Enumerators;

namespace AzureChronos.Functions.Models;

public class AzureChronosSchedule
{
    public DateTime Schedule { get; set; }
    public string CronExpression { get; set; }
    public AzureChronosActions Action { get; set; }
}