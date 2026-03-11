using Flurl.Http;
using OneOf;
using OneOf.Types;

public class GitHub
{
    public async Task<OneOf<PublicKey, Exception>> GetPublicKey(string token)
    {
        try
        {
            var publicKeyData = await "https://api.github.com/repos/KinsonDigital/velo-post/actions/secrets/public-key"
                .WithHeaders(new
                {
                    Accept = "application/vnd.github+json",
                    Authorization = $"Bearer {token}",
                    User_Agent = "VeloPost",
                    X_Github_Api_Version = "2022-11-28",
                })
                .GetJsonAsync<PublicKey>();

            return publicKeyData;
        }
        catch (FlurlHttpException ex)
        {
            return ex;
        }
    }

    public async Task<OneOf<Success, Exception>> UpdateRepoSecret(string secretName, string encryptedValue, string keyId, string token)
    {
        try
        {
            await $"https://api.github.com/repos/KinsonDigital/velo-post/actions/secrets/{secretName}"
                .WithHeaders(new
                {
                    Accept = "application/vnd.github+json",
                    Authorization = $"Bearer {token}",
                    User_Agent = "VeloPost",
                    X_Github_Api_Version = "2022-11-28",
                })
                .PutJsonAsync(new
                {
                    encrypted_value = encryptedValue,
                    key_id = keyId,
                });

            return new Success();
        }
        catch (FlurlHttpException ex)
        {
            return ex;
        }
    }
}
