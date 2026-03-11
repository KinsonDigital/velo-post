using System.Diagnostics;
using System.Text;
using Flurl;
using Flurl.Http;
using Sodium;
using VeloPost;

public class GitHub
{
    public async Task UpdateAccessTokenSecret(string secret)
    {
        var token = Environment.GetEnvironmentVariable("CICD_TOKEN");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("::error::CICD_TOKEN environment variable does not exist.");
            return;
        }

        try
        {
            var publicKeyData = await "https://api.github.com/repos/kinsondigital/velaptor/actions/secrets/public-key"
                .WithHeaders(new
                {
                    Accept = "application/vnd.github+json",
                    Authorization = $"Bearer {token}",
                    User_Agent = "VeloPost",
                    X_Github_Api_Version = "2022-11-28",
                })
                .GetJsonAsync<PublicKey>();

            var publicKey = Convert.FromBase64String(publicKeyData.Key);
            var plaintext = Encoding.UTF8.GetBytes(secret);
            var cipherText = SealedPublicKeyBox.Create(plaintext, publicKey);

            var encryptedValue = Convert.ToBase64String(cipherText);

            
        }
        catch (FlurlHttpException ex)
        {
            var errorResponse = await ex.GetResponseStringAsync();
            Console.WriteLine($"::error::Failed to retrieve public key: {errorResponse}");
        }
    }
}