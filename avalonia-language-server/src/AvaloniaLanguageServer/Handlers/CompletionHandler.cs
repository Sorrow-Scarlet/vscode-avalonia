using AvaloniaLanguageServer.CompletionEngine;
using AvaloniaLanguageServer.Models;
using AvaloniaLanguageServer.Services;

namespace AvaloniaLanguageServer.Handlers;

public class CompletionHandler : CompletionHandlerBase
{
    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
        => Task.FromResult(request);

    public override async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        string? text = _workspace.BufferService.GetTextTillPosition(request.TextDocument.Uri, request.Position);
        if (text == null)
            return new CompletionList();

        var context = await _workspace.GetDocumentContextAsync(request.TextDocument.Uri, _workspaceContext.RootPath);
        if (context.ProjectInfo is not { IsAssemblyExist: true } || context.CompletionMetadata == null)
        {
            return new CompletionList();
        }

        var completionEngine = new AvaloniaLanguageServer.CompletionEngine.CompletionEngine();
        var set = completionEngine.GetCompletions(context.CompletionMetadata, text, text.Length);

        var completions = set?.Completions
            .Where(p => !p.DisplayText.Contains('`'))
            .Select(p => new CompletionItem
            {
                Label = p.DisplayText,
                Detail = p.Description,
                InsertText = p.InsertText,
                Kind = GetCompletionItemKind(p.Kind),
            });


        if (completions == null)
            return new CompletionList(true);

        return new CompletionList(completions, isIncomplete: false);
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions
        (CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = _documentSelector,
            TriggerCharacters = new Container<string>(_triggerChars),
            AllCommitCharacters = new Container<string>("\n"),
            ResolveProvider = false
        };
    }

    public CompletionHandler(Workspace workspace, DocumentSelector documentSelector, WorkspaceContext workspaceContext)
    {
        _workspace = workspace;
        _documentSelector = documentSelector;
        _workspaceContext = workspaceContext;
    }

    static CompletionItemKind GetCompletionItemKind(CompletionKind completionKind)
    {
        string name = Enum.GetName(completionKind) ?? string.Empty;

        var result = name switch
        {
            _ when name.Contains("Property") || name.Contains("AttachedProperty") => CompletionItemKind.Property,
            _ when name.Contains("Event") => CompletionItemKind.Event,
            _ when name.Contains("Namespace") || name.Contains("XmlNamespace") => CompletionItemKind.Module,
            _ when name.Contains("MarkupExtension") => CompletionItemKind.Class,
            _ => GetRest(name)
        };

        return result;

        CompletionItemKind GetRest(string enumName)
        {
            bool success = Enum.TryParse(enumName, out CompletionItemKind kind);
            return success ? kind : CompletionItemKind.Text;
        }
    }

    readonly Workspace _workspace;
    readonly DocumentSelector _documentSelector;
    readonly WorkspaceContext _workspaceContext;

    readonly string[] _triggerChars = { "\'", "\"", " ", "<", ".", "[", "(", "#", "|", "/", "{" };
}
