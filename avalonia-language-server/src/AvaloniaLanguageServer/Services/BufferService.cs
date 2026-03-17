using System.Collections.Concurrent;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace AvaloniaLanguageServer.Services;

public class BufferService
{
    private readonly ConcurrentDictionary<DocumentUri, Buffer> _buffers = new();
    
    public void Add(DocumentUri key, string text)
    {
        _buffers.AddOrUpdate(key, new Buffer(text), (_, _) => new Buffer(text));
    }

    public void Remove(DocumentUri key)
    {
        _buffers.TryRemove(key, out _);
    }

    public string? GetTextTillPosition(DocumentUri key, Position position)
    {
        return _buffers.TryGetValue(key, out var buffer)
            ? buffer.GetTextTillLine(position)
            : null;
    }

    public void ApplyFullChange(DocumentUri key, string text)
    {
        if (_buffers.TryGetValue(key, out var buffer))
        {
            _buffers.TryUpdate(key, new Buffer(text), buffer);
        }
    }
    
    public void ApplyIncrementalChange(DocumentUri key, Range range, string text)
    {
        if (_buffers.TryGetValue(key, out var buffer))
        {
            var newText = Splice(buffer.GetText(), range, text);
            _buffers.TryUpdate(key, new Buffer(newText), buffer);
        }
    }
    
    private static int GetIndex(string buffer, Position position)
    {
        var index = 0;
        for (var i = 0; i < position.Line; ++i)
        {
            index = buffer.IndexOf('\n', index) + 1;
        }
        return index + position.Character;
    }

    private static string Splice(string buffer, Range range, string text)
    {
        var start = GetIndex(buffer, range.Start);
        var end = GetIndex(buffer, range.End);
        return buffer[..start] + text + buffer[end..];
    }
}