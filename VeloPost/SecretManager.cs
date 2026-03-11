using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Flurl;
using Flurl.Http;
using OneOf;
using OneOf.Types;
using Sodium;
using VeloPost;

public class SecretManager
{
    private GitHub github;

    public SecretManager(GitHub github)
    {
        this.github = github;
    }

    public async Task<OneOf<Success, Exception>> UpdateAccessTokenSecretAsync(string secret)
    {
        var token = Environment.GetEnvironmentVariable("CICD_TOKEN");

        if (string.IsNullOrEmpty(token))
        {
            return new Exception("CICD_TOKEN environment variable does not exist.");
        }

        var publicKeyDataResult = await this.github.GetPublicKey(token);

        if (publicKeyDataResult.IsT1)
        {
            return publicKeyDataResult.AsT1;
        }

        var publicKeyData = publicKeyDataResult.AsT0;

        Console.WriteLine($"Key ID: {publicKeyData.KeyId}");
        Console.WriteLine($"Public Key: {publicKeyData.Key}");

        var publicKeyBytes = Convert.FromBase64String(publicKeyData.Key);
        var plainTextBytes = Encoding.UTF8.GetBytes(secret);
        var cipherTextBytes = SealedPublicKeyBox.Create(plainTextBytes, publicKeyBytes);
        var encryptedBase64Value = Convert.ToBase64String(cipherTextBytes);

        var updateRepoResult = await this.github.UpdateRepoSecret(
            "TWITTER_ACCESS_TOKEN",
            encryptedBase64Value,
            publicKeyData.KeyId,
            token);

        return updateRepoResult.Match<OneOf<Success, Exception>>(
            success => success,
            ex => ex
        );
    }
}