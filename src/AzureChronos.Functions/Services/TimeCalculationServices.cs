using System;
using Cronos;

namespace AzureChronos.Functions.Services;

public class TimeCalculationServices
{
    /// <summary>
    /// Function to get the next occurrence of a cron expression
    /// </summary>
    /// <param name="cronExpression"></param>
    /// <returns></returns>
    public DateTime? GetNextOccurrence(string cronExpression)
    {
        var cron = CronExpression.Parse(cronExpression);
        var nextOccurrence = cron.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Utc);
        
        return nextOccurrence;
    }
    
    /// <summary>
    /// Function to validate if a DateTime is within the specified number of minutes from now
    /// </summary>
    /// <param name="dateTime">DateTime to validate. Generated from a CRON expression for example</param>
    /// <param name="validationMinutes"></param>
    /// <returns></returns>
    public bool IsWithinNext30Minutes(DateTime? dateTime, int validationMinutes)
    {
        if (dateTime == null)
        {
            return false;
        }
        
        var now = DateTime.UtcNow;
        var thirtyMinutesFromNow = now.AddMinutes(validationMinutes);
        
        return dateTime >= now && dateTime <= thirtyMinutesFromNow;
    }
}