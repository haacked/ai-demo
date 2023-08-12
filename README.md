# Haacked Woundrous AI Web DEMO

This is a demo of using Open AI in an ASP.NET Core application.

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

## Notes

This project is set up using Central NuGet Package Management.

I also ran `dotnet user-secrets init --project src/AIDemoWeb/AiDemoWeb.csproj` from the repository root to initialize the user secrets.