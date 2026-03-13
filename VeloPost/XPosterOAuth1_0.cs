// X OAuth 1.0a docs: https://docs.x.com/fundamentals/authentication/oauth-1-0a/authorizing-a-request

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VeloPost;

public class XPosterOAuth1_0
{
    private const string TweetUrl = "https://api.twitter.com/2/tweets";

    private readonly string consumerKey;
    private readonly string consumerSecret;
    private readonly string accessToken;
    private readonly string accessTokenSecret;

    public XPosterOAuth1_0(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
    {
        this.consumerKey = consumerKey;
        this.consumerSecret = consumerSecret;
        this.accessToken = accessToken;
        this.accessTokenSecret = accessTokenSecret;
    }

    public async Task<string> PostAsync(string message)
    {
        var oauthParams = GenerateOAuthParameters();
        var signature = CreateSignature("POST", TweetUrl, oauthParams);
        oauthParams["oauth_signature"] = signature;

        var authHeader = BuildAuthorizationHeader(oauthParams);

        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, TweetUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { text = message }),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"Failed to post tweet ({response.StatusCode}):\n{body}");

        return body;
    }

    private SortedDictionary<string, string> GenerateOAuthParameters()
    {
        return new SortedDictionary<string, string>
        {
            ["oauth_consumer_key"] = consumerKey,
            ["oauth_nonce"] = GenerateNonce(),
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ["oauth_token"] = accessToken,
            ["oauth_version"] = "1.0",
        };
    }

    private string CreateSignature(string httpMethod, string baseUrl, SortedDictionary<string, string> oauthParams)
    {
        // 1. Build parameter string: percent-encode keys/values, sort, join with &
        var parameterString = string.Join("&",
            oauthParams.Select(kvp => $"{PercentEncode(kvp.Key)}={PercentEncode(kvp.Value)}"));

        // 2. Build signature base string: METHOD&url&params
        var signatureBaseString = $"{httpMethod.ToUpperInvariant()}&{PercentEncode(baseUrl)}&{PercentEncode(parameterString)}";

        // 3. Build signing key: consumer_secret&token_secret
        var signingKey = $"{PercentEncode(consumerSecret)}&{PercentEncode(accessTokenSecret)}";

        // 4. HMAC-SHA1 hash and base64 encode
        var keyBytes = Encoding.ASCII.GetBytes(signingKey);
        var dataBytes = Encoding.ASCII.GetBytes(signatureBaseString);
        using var hmac = new HMACSHA1(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);

        return Convert.ToBase64String(hash);
    }

    private static string BuildAuthorizationHeader(SortedDictionary<string, string> oauthParams)
    {
        return string.Join(", ",
            oauthParams.Select(kvp => $"{PercentEncode(kvp.Key)}=\"{PercentEncode(kvp.Value)}\""));
    }

    private static string GenerateNonce()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
    }

    /// <summary>
    /// RFC 3986 percent encoding as specified by X's OAuth 1.0a docs.
    /// Unreserved characters (A-Z, a-z, 0-9, '-', '.', '_', '~') are not encoded.
    /// </summary>
    private static string PercentEncode(string value)
    {
        var encoded = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            if (IsUnreserved(b))
                encoded.Append((char)b);
            else
                encoded.Append($"%{b:X2}");
        }
        return encoded.ToString();
    }

    private static bool IsUnreserved(byte b) =>
        b is (>= 0x41 and <= 0x5A)   // A-Z
           or (>= 0x61 and <= 0x7A)   // a-z
           or (>= 0x30 and <= 0x39)   // 0-9
           or 0x2D or 0x2E or 0x5F or 0x7E; // - . _ ~
}
