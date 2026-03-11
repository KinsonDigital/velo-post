// X OAuth 2.0 PKCE flow docs: https://docs.x.com/fundamentals/authentication/oauth-2-0/authorization-code

using Flurl.Http;

namespace VeloPost;

public class XPoster
{
    private const string TweetUrl = "https://api.twitter.com/2/tweets";
    private const string TokenUrl = "https://api.twitter.com/2/oauth2/token";

    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _refreshToken;
    private string _accessToken;

    public XPoster(string accessToken, string clientId, string clientSecret, string refreshToken)
    {
        _accessToken = accessToken;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _refreshToken = refreshToken;
    }

    public async Task<string> PostAsync(string message)
    {
        var response = await TweetUrl
            .WithOAuthBearerToken(_accessToken)
            .PostJsonAsync(new { text = message });

        return await response.GetStringAsync();
    }

    public async Task<string> RefreshAccessTokenAsync()
    {
        var response = await TokenUrl
            .WithBasicAuth(_clientId, _clientSecret)
            .PostUrlEncodedAsync(new
            {
                grant_type = "refresh_token",
                refresh_token = _refreshToken,
            });

        var result = await response.GetJsonAsync<TokenResponse>();
        _accessToken = result.AccessToken;
        _refreshToken = result.RefreshToken;

        return _accessToken;
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public string Scope { get; set; } = string.Empty;

    public string TokenType { get; set; } = string.Empty;
}
