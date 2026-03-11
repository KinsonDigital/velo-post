# velo-post
Social media auto-poster for Velaptor.

**Simple Bot Auth Explanation**

1. Bot starts with: Client ID, Client Secret, and the latest Refresh Token
2. Bot calls X's token endpoint:
   "Hey X, here's my Client ID, Client Secret, and Refresh Token — give me a new Access Token"
3. X responds with:
   - New Access Token (good for 2 hours)
   - New Refresh Token (replaces the old one — save this!)
4. Bot uses the new Access Token to post the tweet
5. Bot saves the new Refresh Token somewhere (env var, file, etc.)
   so it can use it next time

If step 5 fails (you lose the new refresh token), you're locked out and need to run the x-auth.ts utility again.


