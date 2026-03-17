using AvaloniaLanguageServer.Handlers;
using AvaloniaLanguageServer.CompletionEngine.AssemblyMetadata;
using AvaloniaLanguageServer.CompletionEngine.MetadataProviders;
using AvaloniaLanguageServer.Models;
using AvaloniaLanguageServer.Services;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace AvaloniaLanguageServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        InitializeLogging();
        var server = await LanguageServer.From(ConfigureOptions);

        Log.Logger.Information("Language server initialised");
        await server.WaitForExit;
    }

    static void ConfigureOptions(LanguageServerOptions options)
    {
        options
            .WithInput(Console.OpenStandardInput())
            .WithOutput(Console.OpenStandardOutput())
            .ConfigureLogging(p => p
                .AddSerilog(Log.Logger)
                .AddLanguageProtocolLogging()
                .SetMinimumLevel(LogLevel.Trace)
            )
            .WithHandler<CompletionHandler>()
            .WithHandler<TextDocumentSyncHandler>()
            .WithServices(ConfigureServices)
            .OnInitialize((server, request, token) =>
            {
                var workspaceContext = server.Services.GetRequiredService<WorkspaceContext>();
                workspaceContext.Initialize(request);
                return Task.CompletedTask;
            });
    }

    static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(new ConfigurationItem { Section = ServerContract.ConfigurationSection });
        services.AddSingleton(new DocumentSelector(
            new DocumentFilter { Pattern = ServerContract.DocumentPattern, Language = ServerContract.DocumentLanguageId }
        ));
        services.AddSingleton<IMetadataProvider, DnlibMetadataProvider>();
        services.AddSingleton<MetadataReader>();
        services.AddSingleton<CompletionMetadataService>();
        services.AddSingleton<BufferService>();
        services.AddSingleton<WorkspaceContext>();
        services.AddSingleton<Workspace>();
    }

    static void InitializeLogging()
    {
        string logFilePath = Path.Combine(Path.GetTempPath(), ServerContract.DefaultLogFileName);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logFilePath)
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .CreateLogger();
    }
}