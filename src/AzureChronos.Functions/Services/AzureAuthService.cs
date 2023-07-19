using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace AzureChronos.Functions.Services;

public class AzureAuthService
{
    /// <summary>
    /// Gets an Azure authentication token using the DefaultAzureCredential.
    /// </summary>
    /// <returns>The Azure authentication token.</returns>
    public async Task<string> GetAzAuthToken()
        {
            var credential = new DefaultAzureCredential();
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.core.windows.net/.default" });
            var accessToken = await credential.GetTokenAsync(tokenRequestContext);

            if (accessToken.Token == null)
            {
                throw new Exception("Access token could not be acquired.");
            }

            return accessToken.Token;
        }
}

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