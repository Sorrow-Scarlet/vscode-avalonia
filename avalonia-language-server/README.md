# Avalonia Language Server

This directory is the standalone entry point for the Avalonia language server.

## Why this exists

The original repository ships the language server as an implementation detail of the VS Code extension. That makes it harder to:

- develop the server independently from the extension host;
- reuse the same server in other editors;
- define a stable client/server contract for commands, configuration, and startup.

This project is the first step in separating those concerns.

## Current state

The main implementation now lives directly under `avalonia-language-server/src/AvaloniaLanguageServer`.

The completion engine code is now vendored directly into the main implementation project, so this standalone repository only needs two projects: the server and its tests.

The server now keeps document state inside language-server services instead of a single global workspace snapshot, which makes the protocol layer less tied to one host/editor lifecycle.

The VS Code extension now builds the server from this project:

- project: `avalonia-language-server/src/AvaloniaLanguageServer/AvaloniaLanguageServer.csproj`
- output assembly: `avaloniaServer/LanguageServer.dll`
- standalone solution: `avalonia-language-server/AvaloniaLanguageServer.sln`

Keeping the assembly name stable avoids breaking the current extension startup path.

## Near-term extraction plan

1. Continue removing legacy IDE assumptions from the completion engine surface and naming.
2. Add more editor-neutral protocol tests beyond server lifecycle smoke tests.
3. Publish this server as its own repository once the code has fully moved.

## Development

Build the server directly:

```powershell
dotnet build avalonia-language-server/src/AvaloniaLanguageServer/AvaloniaLanguageServer.csproj
```

Build the standalone solution:

```powershell
dotnet build avalonia-language-server/AvaloniaLanguageServer.sln
```

Run the standalone tests:

```powershell
dotnet test avalonia-language-server/AvaloniaLanguageServer.sln
```

Run it over stdio:

```powershell
dotnet run --project avalonia-language-server/src/AvaloniaLanguageServer/AvaloniaLanguageServer.csproj
```