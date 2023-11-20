namespace Haack.AIDemoWeb.Library.Clients;

public record Annotation(
    string Type,
    string Text,
    int StartIndex,
    int EndIndex,
    FileCitation FileCitation);

public record FileCitation(string FileId, string Quote);
