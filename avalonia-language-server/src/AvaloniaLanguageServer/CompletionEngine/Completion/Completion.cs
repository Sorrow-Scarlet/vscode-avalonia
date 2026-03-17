using System;

namespace AvaloniaLanguageServer.CompletionEngine;

[Flags]
public enum CompletionKind
{
    None = 0x0,
    Comment = 0x1,
    Class = 0x2,
    Property = 0x4,
    AttachedProperty = 0x8,
    StaticProperty = 0x10,
    Namespace = 0x20,
    Enum = 0x40,
    MarkupExtension = 0x80,
    Event = 0x100,
    AttachedEvent = 0x200,

    /// <summary>
    /// Properties sourced from DataContexts (view models). Editors may choose
    /// a distinct icon to differentiate data-bound properties from regular ones.
    /// </summary>
    DataProperty = 0x4000,

    /// <summary>
    /// Classes listed from TargetType or Selector context. Editors may choose
    /// a distinct icon to differentiate from <see cref="Class"/> used in tag names.
    /// </summary>
    TargetTypeClass = 0x400,

    /// <summary>
    /// xmlns list completion entries. Historically this used a dedicated icon in editor integrations.
    /// </summary>
    XmlNamespace = 0x800,

    Selector = 0x1000,
    Name = 0x2000,
}

public record Completion(string DisplayText,
    string InsertText,
    string Description,
    CompletionKind Kind,
    int? RecommendedCursorOffset = null,
    string? Suffix = null,
    int? DeleteTextOffset = null,
    byte Priority = 255
    )
{
    public override string ToString() => DisplayText;

    public Completion(string insertText, CompletionKind kind, string? suffix = default, byte priority = 255) :
        this(insertText, insertText, insertText, kind, Suffix: suffix, Priority: priority)
    {

    }

    public Completion(string displayText, string insertText, CompletionKind kind, string? suffix = default, byte priority = 255) :
        this(displayText, insertText, displayText, kind, Priority: priority)
    {

    }

    public bool TriggerCompletionAfterInsert { get; init; }
}
