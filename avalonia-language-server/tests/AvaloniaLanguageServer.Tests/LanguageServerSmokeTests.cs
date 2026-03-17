using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AvaloniaLanguageServer;
using Xunit;

namespace AvaloniaLanguageServer.Tests;

public sealed class LanguageServerSmokeTests
{
    [Fact]
    public async Task LanguageServer_RespondsToInitialize_AndShutsDownCleanly()
    {
        var serverAssemblyPath = typeof(Program).Assembly.Location;

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = Quote(serverAssemblyPath),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();

        try
        {
            await SendMessageAsync(process.StandardInput, new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    processId = Process.GetCurrentProcess().Id,
                    rootUri = (string?)null,
                    capabilities = new { },
                    clientInfo = new { name = "AvaloniaLanguageServer.Tests" }
                }
            });

            using var initializeResponse = await ReadResponseAsync(process.StandardOutput, expectedId: 1, timeoutMs: 10000);
            Assert.True(initializeResponse.RootElement.TryGetProperty("result", out var result));
            Assert.True(result.TryGetProperty("capabilities", out _));

            await SendMessageAsync(process.StandardInput, new
            {
                jsonrpc = "2.0",
                method = "initialized",
                @params = new { }
            });

            await SendMessageAsync(process.StandardInput, new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "shutdown",
                @params = new { }
            });

            using var shutdownResponse = await ReadResponseAsync(process.StandardOutput, expectedId: 2, timeoutMs: 10000);
            Assert.True(shutdownResponse.RootElement.TryGetProperty("result", out _));

            await SendMessageAsync(process.StandardInput, new
            {
                jsonrpc = "2.0",
                method = "exit",
                @params = new { }
            });

            using var exitTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var exited = await WaitForExitAsync(process, exitTokenSource.Token);
            Assert.True(exited, $"Language server did not exit. stderr: {await process.StandardError.ReadToEndAsync()}");
            Assert.Equal(0, process.ExitCode);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            process.Dispose();
        }
    }

    private static async Task SendMessageAsync(StreamWriter writer, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        await writer.WriteAsync($"Content-Length: {bytes.Length}\r\n\r\n{json}");
        await writer.FlushAsync();
    }

    private static async Task<JsonDocument> ReadResponseAsync(StreamReader reader, int expectedId, int timeoutMs)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeoutMs);

        while (true)
        {
            var message = await ReadMessageAsync(reader, cancellationTokenSource.Token);
            using var document = JsonDocument.Parse(message);

            if (!document.RootElement.TryGetProperty("id", out var idProperty) || idProperty.GetInt32() != expectedId)
            {
                continue;
            }

            return JsonDocument.Parse(message);
        }
    }

    private static async Task<string> ReadMessageAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var contentLength = 0;

        while (true)
        {
            var headerLine = await ReadLineAsync(reader, cancellationToken);
            if (string.IsNullOrEmpty(headerLine))
            {
                break;
            }

            if (headerLine.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                contentLength = int.Parse(headerLine["Content-Length:".Length..].Trim());
            }
        }

        if (contentLength <= 0)
        {
            throw new InvalidOperationException("Missing Content-Length header in server response.");
        }

        var buffer = new char[contentLength];
        var read = 0;

        while (read < contentLength)
        {
            var chunk = await reader.ReadAsync(buffer.AsMemory(read, contentLength - read), cancellationToken);
            if (chunk == 0)
            {
                throw new InvalidOperationException("Unexpected end of stream while reading server response body.");
            }

            read += chunk;
        }

        return new string(buffer);
    }

    private static async Task<string?> ReadLineAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        return await reader.ReadLineAsync(cancellationToken);
    }

    private static async Task<bool> WaitForExitAsync(Process process, CancellationToken cancellationToken)
    {
        try
        {
            await process.WaitForExitAsync(cancellationToken);
            return true;
        }

        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private static string Quote(string value) => $"\"{value}\"";
}