using System;
using Cronos;
using NCrontab;

namespace AzureChronos.Functions.Services;

public class TimeCalculationServices
{
    /// <summary>
    /// Calculates the next occurrence of a cron expression in UTC time zone.
    /// </summary>
    /// <param name="cronExpression">The cron expression to evaluate.</param>
    /// <returns>The next occurrence of the cron expression in UTC time zone, or null if the expression is invalid.</returns>
    public static DateTime? GetNextOccurrence(string cronExpression)
    {
        try
        {
            var expression = CrontabSchedule.Parse(cronExpression);
            var nextOccurrence = expression.GetNextOccurrence(DateTime.UtcNow);
            return nextOccurrence;
        }
        catch (Exception)
        {
            return null; 
        }
        
    }
    
    /// <summary>
    /// Determines whether the specified date and time is within the next specified number of minutes from the current UTC date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to compare.</param>
    /// <param name="validationMinutes">The number of minutes to validate.</param>
    /// <returns>true if the specified date and time is within the next specified number of minutes from the current UTC date and time; otherwise, false.</returns>
    public static bool IsWithinNext30Minutes(DateTime? dateTime, int validationMinutes)
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