using Microsoft.AspNetCore.Authentication;

namespace AIDemo.Blazor.Library;

public class TokenProvider(IHttpContextAccessor httpContextAccessor)
{
    public async Task<string?> GetAccessTokenAsync(string scheme)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }
        var authenticateResult = await httpContext.AuthenticateAsync(scheme);
        return authenticateResult.Properties?.GetTokenValue("access_token");
    }
}