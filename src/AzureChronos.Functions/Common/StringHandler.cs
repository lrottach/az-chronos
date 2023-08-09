using System;
using System.Text.RegularExpressions;

namespace AzureChronos.Functions.Common;

/// <summary>
/// The StringHandler class is a static class that provides static methods to work with strings.
/// </summary>
public static class StringHandler
{
    /// <summary>
    /// Extracts the resource group name from the given Azure resource ID.
    /// </summary>
    /// <param name="resourceId">The resource ID from which the resource group name is to be extracted.</param>
    /// <returns>Returns the name of the resource group if the matching is successful, otherwise, returns null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided resourceId is null or whitespace.</exception>
    public static string ExtractResourceGroupName(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentNullException(nameof(resourceId));

        var match = Regex.Match(resourceId, @"/resourceGroups/(?<resourceGroupName>[^/]+)/", RegexOptions.IgnoreCase);

        return match.Success ? match.Groups["resourceGroupName"].Value : null;
    }
}