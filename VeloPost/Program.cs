using System.Diagnostics;
using System.Text;
using Flurl;
using Flurl.Http;
using Sodium;
using VeloPost;


var token = Environment.GetEnvironmentVariable("CICD_TOKEN") ?? "";

var publicKeyData = await "https://api.github.com/repos/kinsondigital/velaptor/actions/secrets/public-key"
    .WithHeaders(new
    {
        Accept = "application/vnd.github+json",
        Authorization = $"Bearer {token}",
        User_Agent = "VeloPost",
        X_Github_Api_Version = "2022-11-28",
    })
    .GetJsonAsync<PublicKey>();

byte[] publicKey = Convert.FromBase64String(publicKeyData.Key);
byte[] plaintext = Encoding.UTF8.GetBytes("my-secret-value");
byte[] ciphertext = SealedPublicKeyBox.Create(plaintext, publicKey);

string encryptedValue = Convert.ToBase64String(ciphertext);

Console.WriteLine(encryptedValue);

Debugger.Break();

public class PublicKey
{
    public string KeyId { get; set; }

    public string Key { get; set; }
}

// var accessToken = "";
// var clientId = "";
// var clientSecret = "";

// var poser = new XPoster(accessToken, clientId, clientSecret, refreshToken);

// var refreshToken = await poser.RefreshAccessTokenAsync();
// var response = await poser.PostAsync("Test from velaptor");

// Debugger.Break();

// ----------------------------------------------------

// var token = Environment.GetEnvironmentVariable("CICD_TOKEN");
// var issue = await "https://api.github.com/repos/kinsondigital/velaptor/issues/1164"
//     .WithHeaders(new
//     {
//         Accept = "application/vnd.github+json",
//         Authorization = $"Bearer {token}",
//         User_Agent = "VeloPost",
//         X_Github_Api_Version = "2022-11-28",
//     })
//     .GetJsonAsync<Issue>();

// Debugger.Break();


// public class Issue
// {
//     public long Id { get; set; }

//     public string Url { get; set; }

//     public string Title { get; set; }
// }

