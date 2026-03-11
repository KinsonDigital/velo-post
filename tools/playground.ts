import { getPublicKey, getVarSecret } from "./github.ts";

const token = Deno.env.get("CICD_TOKEN") ?? "";
// const secret = await getVarSecret("TEST_SECRET", token);

const publicKey = await getPublicKey(token);

console.log(publicKey);
