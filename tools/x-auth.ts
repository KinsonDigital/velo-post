/**
 * One-time OAuth 2.0 Authorization Code with PKCE utility for X/Twitter.
 *
 * Usage:
 *   deno run --allow-net --allow-run x-auth.ts <CLIENT_ID> <CLIENT_SECRET>
 *
 * Prerequisites:
 *   - Add http://localhost:3000/callback as a redirect URI in your X Developer Console OAuth 2.0 settings.
 *   - Your app must have the "tweet.read" and "tweet.write" scopes enabled.
 */

const CLIENT_ID = Deno.args[0];
const CLIENT_SECRET = Deno.args[1];
const REDIRECT_URI = "http://localhost:3000/callback";
const SCOPES = "tweet.read tweet.write users.read offline.access";

if (!CLIENT_ID || !CLIENT_SECRET) {
  console.error("Usage: deno run --allow-net --allow-run x-auth.ts <CLIENT_ID> <CLIENT_SECRET>");
  Deno.exit(1);
}

// Generate PKCE code verifier (43-128 chars, URL-safe)
function generateCodeVerifier(): string {
  const bytes = new Uint8Array(32);
  crypto.getRandomValues(bytes);
  return base64UrlEncode(bytes);
}

// SHA-256 hash the verifier to create the code challenge
async function generateCodeChallenge(verifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(verifier);
  const digest = await crypto.subtle.digest("SHA-256", data);
  return base64UrlEncode(new Uint8Array(digest));
}

function base64UrlEncode(bytes: Uint8Array): string {
  const binString = Array.from(bytes, (b) => String.fromCharCode(b)).join("");
  return btoa(binString).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function generateState(): string {
  const bytes = new Uint8Array(16);
  crypto.getRandomValues(bytes);
  return base64UrlEncode(bytes);
}

// --- Main flow ---

const codeVerifier = generateCodeVerifier();
const codeChallenge = await generateCodeChallenge(codeVerifier);
const state = generateState();

const authUrl = new URL("https://twitter.com/i/oauth2/authorize");
authUrl.searchParams.set("response_type", "code");
authUrl.searchParams.set("client_id", CLIENT_ID);
authUrl.searchParams.set("redirect_uri", REDIRECT_URI);
authUrl.searchParams.set("scope", SCOPES);
authUrl.searchParams.set("state", state);
authUrl.searchParams.set("code_challenge", codeChallenge);
authUrl.searchParams.set("code_challenge_method", "S256");

console.log("\n=== X/Twitter OAuth 2.0 PKCE Authorization ===\n");
console.log("Opening browser to authorize...\n");
console.log("If the browser doesn't open, visit this URL manually:\n");
console.log(authUrl.toString());
console.log("\nWaiting for callback on http://localhost:3000/callback ...\n");

// Try to open the browser
try {
  Deno.build.os === "windows"
    ? Deno.spawn("rundll32", ["url.dll,FileProtocolHandler", authUrl.toString()])
    : Deno.spawn("open", [authUrl.toString()]);
} catch {
  // Browser open failed — user can use the printed URL
}

// Start local server to capture the callback
const server = Deno.serve({ port: 3000, hostname: "localhost" }, async (req) => {
  const url = new URL(req.url);

  if (url.pathname !== "/callback") {
    return new Response("Not found", { status: 404 });
  }

  const code = url.searchParams.get("code");
  const returnedState = url.searchParams.get("state");

  if (!code) {
    return new Response("Error: No authorization code received.", { status: 400 });
  }

  if (returnedState !== state) {
    return new Response("Error: State mismatch — possible CSRF attack.", { status: 400 });
  }

  console.log("Authorization code received. Exchanging for tokens...\n");

  // Exchange the code for tokens
  const basicAuth = btoa(`${CLIENT_ID}:${CLIENT_SECRET}`);
  const tokenResponse = await fetch("https://api.twitter.com/2/oauth2/token", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      Authorization: `Basic ${basicAuth}`,
    },
    body: new URLSearchParams({
      code,
      grant_type: "authorization_code",
      redirect_uri: REDIRECT_URI,
      code_verifier: codeVerifier,
    }),
  });

  const tokenData = await tokenResponse.json();

  if (!tokenResponse.ok) {
    console.error("Token exchange failed:", tokenData);
    server.shutdown();
    return new Response("Token exchange failed. Check the terminal.", { status: 500 });
  }

  console.log("=== Tokens received successfully! ===\n");
  console.log(`ACCESS_TOKEN=${tokenData.access_token}\n`);
  console.log(`REFRESH_TOKEN=${tokenData.refresh_token}\n`);
  console.log(`EXPIRES_IN=${tokenData.expires_in} seconds\n`);
  console.log(`SCOPE=${tokenData.scope}\n`);
  console.log("Save these as environment variables for your bot.");
  console.log("The refresh token does not expire as long as you keep using it.\n");

  // Shut down after a short delay to let the response go through
  setTimeout(() => server.shutdown(), 500);

  return new Response(
    "<html><body><h2>Authorization successful!</h2><p>You can close this tab. Check the terminal for your tokens.</p></body></html>",
    { headers: { "Content-Type": "text/html" } },
  );
});

console.log("Local server running on http://localhost:3000 ...");

await server.finished;
