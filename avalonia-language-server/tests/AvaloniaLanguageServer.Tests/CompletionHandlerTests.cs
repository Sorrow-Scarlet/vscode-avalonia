using AvaloniaLanguageServer.CompletionEngine;
using AvaloniaLanguageServer.Handlers;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace AvaloniaLanguageServer.Tests;

public sealed class CompletionHandlerTests
{
    [Fact]
    public void BuildInsertText_WrapsElementCompletionAsOpenClosePair()
    {
        var result = CompletionHandler.BuildInsertText("Button", CompletionKind.Class, prependOpenBracket: true);

        Assert.Equal("<Button>$0</Button>", result.InsertText);
        Assert.Equal(InsertTextFormat.Snippet, result.InsertTextFormat);
    }

    [Fact]
    public void BuildInsertText_DoesNotWrapPropertyCompletion()
    {
        var result = CompletionHandler.BuildInsertText("Background=\"\"", CompletionKind.Property, prependOpenBracket: true);

        Assert.Equal("<Background=\"\"", result.InsertText);
        Assert.Equal(InsertTextFormat.PlainText, result.InsertTextFormat);
    }

    [Fact]
    public void BuildInsertText_WrapsGenericElementUsingTagNameOnlyForClosingTag()
    {
        var result = CompletionHandler.BuildInsertText("MyControl x:TypeArguments=\"\"", CompletionKind.Class, prependOpenBracket: true);

        Assert.Equal("<MyControl x:TypeArguments=\"\">$0</MyControl>", result.InsertText);
        Assert.Equal(InsertTextFormat.Snippet, result.InsertTextFormat);
    }

    [Fact]
    public void BuildInsertText_LeavesNonFallbackInsertUnchanged()
    {
        var result = CompletionHandler.BuildInsertText("Button", CompletionKind.Class, prependOpenBracket: false);

        Assert.Equal("Button", result.InsertText);
        Assert.Equal(InsertTextFormat.PlainText, result.InsertTextFormat);
    }
}
