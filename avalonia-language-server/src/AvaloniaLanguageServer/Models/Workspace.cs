using System.Collections.Concurrent;
using AvaloniaLanguageServer.Services;

namespace AvaloniaLanguageServer.Models;

public class Workspace
{
    public Workspace(BufferService bufferService, CompletionMetadataService completionMetadataService)
    {
        BufferService = bufferService;
        _completionMetadataService = completionMetadataService;
    }

    public BufferService BufferService { get; }

    public async Task<DocumentContext> OpenDocumentAsync(DocumentUri uri, string? rootPath)
    {
        var context = await CreateDocumentContextAsync(uri, rootPath);
        _documents[uri] = context;
        return context;
    }

    public async Task<DocumentContext> GetDocumentContextAsync(DocumentUri uri, string? rootPath)
    {
        if (_documents.TryGetValue(uri, out var context))
        {
            return context;
        }

        return await OpenDocumentAsync(uri, rootPath);
    }

    public bool RemoveDocument(DocumentUri uri)
    {
        return _documents.TryRemove(uri, out _);
    }

    private async Task<DocumentContext> CreateDocumentContextAsync(DocumentUri uri, string? rootPath)
    {
        try
        {
            var projectInfo = await ProjectInfo.GetProjectInfoAsync(uri);
            var completionMetadata = _completionMetadataService.GetForRootPath(rootPath);
            return new DocumentContext(uri, projectInfo, completionMetadata);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to initialize workspace: {uri}", e);
        }
    }

    private readonly ConcurrentDictionary<DocumentUri, DocumentContext> _documents = new();
    private readonly CompletionMetadataService _completionMetadataService;
}