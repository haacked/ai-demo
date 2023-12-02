namespace Haack.AIDemoWeb.Library.Clients;

public record BlazorMessage(string Text, string Author, bool IsUser, DateTime Created);