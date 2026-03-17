namespace AvaloniaLanguageServer.Services;

public sealed class WorkspaceContext
{
    public string? RootPath { get; private set; }

    public void Initialize(InitializeParams request)
    {
        RootPath = request.RootUri?.GetFileSystemPath() ?? request.RootPath;
    }
}