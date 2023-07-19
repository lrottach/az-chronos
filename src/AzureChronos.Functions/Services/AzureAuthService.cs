using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace AzureChronos.Functions.Services;

public class AzureAuthService
{
    public async Task<string> GetAzureAuthToken()
    {
        var credentials = new DefaultAzureCredential();
        var token = await credentials.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
        
        if (token.Token == null)
        {
            throw new Exception("Could not retrieve Azure AD token.");
        }
        
        return token.Token;
    }
}