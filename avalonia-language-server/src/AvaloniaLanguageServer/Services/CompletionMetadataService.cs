using System.Text.Json;
using AvaloniaLanguageServer.CompletionEngine;
using AvaloniaLanguageServer.CompletionEngine.AssemblyMetadata;
using AvaloniaLanguageServer.Models;

namespace AvaloniaLanguageServer.Services;

public sealed class CompletionMetadataService
{
    public CompletionMetadataService(MetadataReader metadataReader)
    {
        _metadataReader = metadataReader;
    }

    public Metadata? GetForRootPath(string? rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            return null;

        var solutionName = ResolveSolutionName(rootPath);
        if (string.IsNullOrWhiteSpace(solutionName))
            return null;

        var solutionSnapshotPath = Path.Combine(Path.GetTempPath(), $"{solutionName}.json");
        if (!File.Exists(solutionSnapshotPath))
            return null;

        var content = File.ReadAllText(solutionSnapshotPath);
        var package = JsonSerializer.Deserialize<SolutionData>(content);
        var executableProject = package?.GetExecutableProject();
        if (executableProject == null || string.IsNullOrWhiteSpace(executableProject.TargetPath))
            return null;

        return _metadataReader.GetForTargetAssembly(executableProject.TargetPath);
    }

    private static string? ResolveSolutionName(string rootPath)
    {
        var solutionPath = Directory
            .EnumerateFiles(rootPath, "*.sln", SearchOption.AllDirectories)
            .FirstOrDefault();

        return solutionPath != null
            ? Path.GetFileName(solutionPath)
            : Path.GetFileNameWithoutExtension(rootPath);
    }

    private readonly MetadataReader _metadataReader;
}