Set-Location "./src/vscode-avalonia"
yarn install

Set-Location ..

Write-Host $PWD

# Build Avalonia LSP
dotnet build "$PWD/AvaloniaLSP/AvaloniaLanguageServer/AvaloniaLanguageServer.csproj" /property:GenerateFullPaths=true --output "$PWD/vscode-avalonia/avaloniaServer"

# Build Solution parser
dotnet build "$PWD/SolutionParser/SolutionParser.csproj" /property:GenerateFullPaths=true --output "$PWD/vscode-avalonia/solutionParserTool"

Write-Host "🎉 Great success"