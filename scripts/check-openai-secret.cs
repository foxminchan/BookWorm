#:property ManagePackageVersionsCentrally=false
#:package Spectre.Console@0.50.0
#:package CliWrap@3.6.6

using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

const string ProjectPath = "src/Aspire/BookWorm.AppHost/BookWorm.AppHost.csproj";
const string SecretKey = "Parameters:openai-openai-apikey";
const string ApiKeyUrl = "https://platform.openai.com/api-keys";

AnsiConsole.MarkupLine("[cyan]Checking for OpenAI API key in user secrets...[/]");

// Get current secrets
var listResult = await Cli.Wrap("dotnet")
    .WithArguments(["user-secrets", "list", "--project", ProjectPath])
    .WithValidation(CommandResultValidation.None)
    .ExecuteBufferedAsync();

// Check if the OpenAI API key is set
if (
    listResult.StandardOutput.Contains(SecretKey)
    && listResult.StandardOutput.Contains("=")
    && !string.IsNullOrWhiteSpace(GetSecretValue(listResult.StandardOutput, SecretKey))
)
{
    AnsiConsole.MarkupLine("[green]✓ OpenAI API key is already configured.[/]");
    return 0;
}

AnsiConsole.MarkupLine("[yellow]⚠ OpenAI API key is not configured.[/]");
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[cyan]Please enter your OpenAI API key (or press Ctrl+C to cancel):[/]");
AnsiConsole.MarkupLine($"[grey]You can get your API key from: {ApiKeyUrl}[/]");
AnsiConsole.WriteLine();

// Prompt for API key securely
var apiKey = AnsiConsole.Prompt(
    new TextPrompt<string>("[cyan]OpenAI API Key:[/]")
        .Secret()
        .Validate(key =>
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return ValidationResult.Error("[red]API key cannot be empty[/]");
            }

            // OpenAI API keys start with "sk-" or "sk-proj-" and are typically 48-51 characters
            if (!key.StartsWith("sk-"))
            {
                return ValidationResult.Error(
                    "[red]Invalid API key format. OpenAI keys should start with 'sk-'[/]"
                );
            }

            if (key.Length < 20)
            {
                return ValidationResult.Error("[red]API key is too short[/]");
            }

            return ValidationResult.Success();
        })
);

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[cyan]Setting OpenAI API key...[/]");

var setResult = await Cli.Wrap("dotnet")
    .WithArguments(["user-secrets", "set", SecretKey, apiKey, "--project", ProjectPath])
    .WithValidation(CommandResultValidation.None)
    .ExecuteBufferedAsync();

if (setResult.ExitCode == 0)
{
    AnsiConsole.MarkupLine("[green]✓ OpenAI API key has been configured successfully.[/]");
    return 0;
}
else
{
    AnsiConsole.MarkupLine("[red]✗ Failed to set OpenAI API key.[/]");
    if (!string.IsNullOrWhiteSpace(setResult.StandardError))
    {
        AnsiConsole.MarkupLine($"[red]{setResult.StandardError}[/]");
    }
    return 1;
}

static string GetSecretValue(string output, string key)
{
    var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
    var line = lines.FirstOrDefault(l => l.Contains(key));

    if (line is null)
        return string.Empty;

    var parts = line.Split('=', 2);
    return parts.Length == 2 ? parts[1].Trim() : string.Empty;
}
