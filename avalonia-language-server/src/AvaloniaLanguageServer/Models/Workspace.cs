using System.Text.Json;
using AvaloniaLanguageServer.CompletionEngine;
using AvaloniaLanguageServer.CompletionEngine.AssemblyMetadata;
using AvaloniaLanguageServer.CompletionEngine.MetadataProviders;
using AvaloniaLanguageServer.Services;

namespace AvaloniaLanguageServer.Models;

public class Workspace
{
    public ProjectInfo? ProjectInfo { get; private set; }
    public BufferService BufferService { get; } = new();

    public async Task InitializeAsync(DocumentUri uri, string? rootPath)
    {
        try
        {
            ProjectInfo = await ProjectInfo.GetProjectInfoAsync(uri);
            CompletionMetadata = BuildCompletionMetadata(rootPath);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to initialize workspace: {uri}", e);
        }
    }

    Metadata? BuildCompletionMetadata(string? rootPath)
    {
        if (rootPath == null)
            return null;

        var slnFile = SolutionName(rootPath) ?? Path.GetFileNameWithoutExtension(rootPath);
        if (slnFile == null)
            return null;

        var slnFilePath = Path.Combine(Path.GetTempPath(), $"{slnFile}.json");
        if (!File.Exists(slnFilePath))
            return null;

        string content = File.ReadAllText(slnFilePath);
        var package = JsonSerializer.Deserialize<SolutionData>(content);
        var exeProj = package!.GetExecutableProject();

        return _metadataReader.GetForTargetAssembly(exeProj?.TargetPath ?? string.Empty);
    }

    string? SolutionName(string rootPath)
    {
        var slnFiles = Directory.EnumerateFiles(rootPath, "*.sln", SearchOption.AllDirectories);
        foreach (string slnFile in slnFiles)
        {
            return Path.GetFileName(slnFile);
        }

        return null;
    }

    public Metadata? CompletionMetadata { get; private set; }

    readonly MetadataReader _metadataReader = new(new DnlibMetadataProvider());
}