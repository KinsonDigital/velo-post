

export async function getVarSecret(name: string, token: string): Promise<string> {
    const baseUrl = "https://api.github.com";
    const orgName = "KinsonDigital";
    const repoName = "velo-post";
    const url = `${baseUrl}/repos/${orgName}/${repoName}/actions/secrets/${name}`;

    const response = await fetch(url, {
        method: "GET",
        headers: {
            Accept: "application/vnd.github+json",
            Authorization: `Bearer ${token}`,
            "X-GitHub-Api-Version": "2022-11-28",
        }
    });

    const data = await response.json();

    console.log(JSON.stringify(data, null, 2));

    return "";
}

export async function getPublicKey(token: string): Promise<string> {
    const baseUrl = "https://api.github.com";
    const orgName = "KinsonDigital";
    const repoName = "velo-post";
    const url = `${baseUrl}/repos/${orgName}/${repoName}/actions/secrets/public-key`;

    const response = await fetch(url, {
        method: "GET",
        headers: {
            Accept: "application/vnd.github+json",
            Authorization: `Bearer ${token}`,
            "X-GitHub-Api-Version": "2022-11-28",
        }
    });

    const data = await response.json();

    console.log(JSON.stringify(data, null, 2));

    return "";
}