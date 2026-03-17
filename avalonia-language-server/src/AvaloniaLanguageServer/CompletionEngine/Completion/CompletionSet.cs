using System.Collections.Generic;

namespace AvaloniaLanguageServer.CompletionEngine;

public class CompletionSet
{
    public int StartPosition { get; set; }

    public List<Completion> Completions { get; set; } = new();
}
