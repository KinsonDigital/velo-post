// X OAuth 2.0 PKCE flow docs: https://docs.x.com/fundamentals/authentication/oauth-2-0/authorization-code

using System.Text.Json.Serialization;
using Flurl.Http;

namespace VeloPost;

/**
Workflow:
Bootstrap (one-time, in browser)
  → PKCE flow → receive access_token + refresh_token
  → Store BOTH somewhere secure (e.g., GitHub secrets, env vars)

Every time you want to post (even days/months later):
  1. Load the stored refresh_token
  2. Call RefreshAccessTokenAsync(refreshToken)
     → Returns a new access_token + a new refresh_token
  3. Store the NEW refresh_token (replaces the old one)
  4. Call PostAsync(message, newAccessToken)
*/

public enum OAuthType
{
    OAuth1_0,
    OAuth2_0PKCE,
}

public class XPosterOAuth2_0
{
    private const string TweetUrl = "https://api.twitter.com/2/tweets";
    private const string TokenUrl = "https://api.twitter.com/2/oauth2/token";

    private readonly string clientId;
    private readonly string clientSecret;

    public XPosterOAuth2_0(string clientId, string clientSecret)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
    }

    public async Task<string> PostAsync(string message, string refreshToken)
    {
        var (accessToken, newRefreshToken) = await RefreshAccessTokenAsync(refreshToken);

        Console.WriteLine($"New Access Token: {accessToken}");
        Console.WriteLine($"New Refresh Token: {newRefreshToken}");

        try
        {
            var response = await TweetUrl
                .WithOAuthBearerToken(accessToken)
                .PostJsonAsync(new { text = message });

            // TODO: Store the new refresh token in GitHub secrets

            return await response.GetStringAsync();
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            Console.WriteLine($"Failed to post tweet:\n{body}");

            return string.Empty;
        }
    }

    private async Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            var response = await TokenUrl
                .WithBasicAuth(clientId, clientSecret)
                .PostUrlEncodedAsync(new
                {
                    grant_type = "refresh_token",
                    refresh_token = refreshToken,
                });

            var result = await response.GetJsonAsync<TokenResponse>();
            var accessToken = result.AccessToken;
            refreshToken = result.RefreshToken;

            Console.WriteLine($"Token scope: {result.Scope}");

            return (accessToken, refreshToken);
        }
        catch (FlurlHttpException ex)
        {
            var body = await ex.GetResponseStringAsync();
            Console.WriteLine($"Failed to refresh access token:\n{body}");

            return (string.Empty, refreshToken);
        }
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}
