using System.Diagnostics;
using System.Text;
using Flurl;
using Flurl.Http;
using OneOf.Types;
using Sodium;
using VeloPost;


var token = Environment.GetEnvironmentVariable("CICD_TOKEN") ?? "";

var github = new GitHub();
var secretManager = new SecretManager(github);

// var updateSecretResult = await secretManager.UpdateAccessTokenSecretAsync("calvin");

// updateSecretResult.Switch(
//     success => Console.WriteLine("Secret updated successfully."),
//     ex => Console.WriteLine($"Failed to update secret: {ex.Message}")
// );

// var clientId = "";
// var clientSecret = "";
// var refreshToken = "";
// var poster = new XPosterOAuth2_0(clientId, clientSecret);
// await poster.PostAsync("test from velo-post", refreshToken);

var consumerKey = Environment.GetEnvironmentVariable("X_CONSUMER_API_KEY");
var consumerSecret = Environment.GetEnvironmentVariable("X_CONSUMER_API_SECRET");
var accessToken = Environment.GetEnvironmentVariable("X_ACCESS_TOKEN_KEY");
var accessTokenSecret = Environment.GetEnvironmentVariable("X_ACCESS_TOKEN_SECRET");

var poster = new XPosterOAuth1_0(consumerKey, consumerSecret, accessToken, accessTokenSecret);

await poster.PostAsync("test from velo-post");
