import { XClient } from "jsr:@kinsondigital/kd-clients@1.0.0-preview.15/social";

// TODO: Need to import this from kd-clients library once it is available
export interface XAuthValues {
	/**
	 * Gets or sets the consumer key.
	 */
	consumer_api_key: string;

	/**
	 * Gets or sets the consumer secret.
	 */
	consumer_api_secret: string;

	/**
	 * Gets or sets the access token key.
	 */
	access_token_key: string;

	/**
	 * Gets or sets the access token secret.
	 */
	access_token_secret: string;
}

const consumerAPIKey = Deno.env.get("X_CONSUMER_API_KEY") ?? "";
const consumerAPISecret = Deno.env.get("X_CONSUMER_API_SECRET") ?? "";
const accessTokenKey = Deno.env.get("X_ACCESS_TOKEN_KEY") ?? "";
const accessTokenSecret = Deno.env.get("X_ACCESS_TOKEN_SECRET") ?? "";

const authValues: XAuthValues = {
	consumer_api_key: consumerAPIKey,
	consumer_api_secret: consumerAPISecret,
	access_token_key: accessTokenKey,
	access_token_secret: accessTokenSecret,
};

const post = "test from kinson digital";

const xClient: XClient = new XClient(authValues);
await xClient.tweet(post);

export function isNothing<T>(
	value: unknown | undefined | null | string | number | boolean | T[],
): value is undefined | null | "" {
	if (value === undefined || value === null) {
		return true;
	}

	if (typeof value === "string") {
		return value === "";
	}

	if (typeof value === "number") {
		return isNaN(value);
	}

	if (Array.isArray(value)) {
		return value.length <= 0;
	}

	return false;
}
