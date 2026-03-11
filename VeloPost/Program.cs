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

var updateSecretResult = await secretManager.UpdateAccessTokenSecretAsync("calvin");

updateSecretResult.Switch(
    success => Console.WriteLine("Secret updated successfully."),
    ex => Console.WriteLine($"Failed to update secret: {ex.Message}")
);
