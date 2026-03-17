using AvaloniaLanguageServer.CompletionEngine;

namespace AvaloniaLanguageServer.Models;

public sealed class DocumentContext
{
    public DocumentContext(DocumentUri uri, ProjectInfo? projectInfo, Metadata? completionMetadata)
    {
        Uri = uri;
        ProjectInfo = projectInfo;
        CompletionMetadata = completionMetadata;
    }

    public DocumentUri Uri { get; }

    public ProjectInfo? ProjectInfo { get; }

    public Metadata? CompletionMetadata { get; }
}