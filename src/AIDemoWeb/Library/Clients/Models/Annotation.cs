namespace Haack.AIDemoWeb.Library.Clients;

/// <summary>
/// An annotation associated with message content.
/// </summary>
/// <param name="Type">Either <c>file_citation</c> or <c>file_path</c>.</param>
/// <param name="Text">The text in the message content that needs to be replaced.</param>
/// <param name="StartIndex">Index of the start of the citation.</param>
/// <param name="EndIndex">Index of the end of the citation.</param>
/// <param name="FileCitation">
/// A citation within the message that points to a specific quote from a specific File associated with the assistant
/// or the message. Generated when the assistant uses the "retrieval" tool to search files. This is only present if
/// <see cref="Type"/> is <c>file_citation</c>.
/// </param>
/// <param name="FilePath">
/// A URL for the file that's generated when the assistant used the code_interpreter tool to generate a file. Only
/// present when <see cref="Type"/> is <c>file_path</c>.
/// </param>
public record Annotation(
    string Type,
    string Text,
    int StartIndex,
    int EndIndex,
    FileCitation? FileCitation,
    FileReference? FilePath);

public record FileCitation(string FileId, string Quote) : FileReference(FileId);
