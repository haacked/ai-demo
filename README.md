# Haacked Woundrous AI Web DEMO

This is a demo of using Open AI in a Blazor Web application.

## Setup

You'll need to create an account on https://chat.openai.com/.

Once you have an account, you'll need to create an API key.

Then you'll need to set the following user secrets:

```bash
script/user-secrets set OpenAI:ApiKey $OpenAI_ApiKey
```

And set up the following environment variables:

```bash
OpenAI_OrganizationId={Your org id}
```

This also requires setting up a [GitHub OAuth App](https://github.com/settings/developers) for authentication.

And to enable others to participate, I'm using ngrok: http://bit.ly/haack-ai (redirects to https://devoted-upright-bluejay.ngrok-free.app/) to expose my local web server to the internet.

This also requires setting up a [GitHub OAuth App](https://github.com/settings/developers) for authentication.

You can point that app to https://localhost:7047/signin-github for the callback URL.

To enable others to participate, I'm using ngrok: http://bit.ly/haack-ai (redirects to https://devoted-upright-bluejay.ngrok-free.app/) to expose my local web server to the internet.
If you want to do the same, make sure to set the following environment variable:

```bash
GitHub_Host={Your ngrok host name}
```

## Running

This app is built on .NET Aspire. To run the app, select `AIdemo.AppHost` and hit play (`F5`).

## Notes

This project is set up using Central NuGet Package Management.

I also ran `dotnet user-secrets init --project src/AIDemo.Web/AIDemo.Web.csproj` from the repository root to initialize the user secrets.
