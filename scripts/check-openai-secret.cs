#!/usr/bin/env dotnet

#:property ManagePackageVersionsCentrally=false
#:package Spectre.Console@0.54.0
#:package Microsoft.Extensions.Configuration.UserSecrets@10.0.0

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

// Find your User Secrets ID in your `BookWorm.AppHost.csproj` file
const string UserSecretsId = "78a33324-5add-4731-b2fc-e0fc06ef0564";
const string SecretKey = "Parameters:openai-openai-apikey";
const string ApiKeyUrl = "https://platform.openai.com/api-keys";

AnsiConsole.MarkupLine("[cyan]Checking for OpenAI API key in user secrets...[/]");

// Build configuration to read user secrets
var configuration = new ConfigurationBuilder().AddUserSecrets(UserSecretsId).Build();

// Check if the OpenAI API key is set
var existingApiKey = configuration[SecretKey];
if (!string.IsNullOrWhiteSpace(existingApiKey))
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
var apiKey = await AnsiConsole.PromptAsync(
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

// Get the secrets file path
var secretsPath = GetSecretsFilePath(UserSecretsId);
var secretsDir = Path.GetDirectoryName(secretsPath);

if (!Directory.Exists(secretsDir))
{
    Directory.CreateDirectory(secretsDir!);
}

// Read existing secrets or create new dictionary
var secrets = new Dictionary<string, string?>();
if (File.Exists(secretsPath))
{
    var existingJson = await File.ReadAllTextAsync(secretsPath);
    try
    {
        secrets =
            JsonSerializer.Deserialize(existingJson, SecretsJsonContext.Default.StringDictionary)
            ?? [];
    }
    catch (JsonException ex)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠ Existing secrets file is invalid: {ex.Message}[/]");
        AnsiConsole.MarkupLine("[yellow]  Starting with empty secrets.[/]");
        secrets = [];
    }
}

// Update the API key
secrets[SecretKey] = apiKey;

// Write back to secrets file
try
{
    var json = JsonSerializer.Serialize(secrets, SecretsJsonContext.Default.StringDictionary);
    await File.WriteAllTextAsync(secretsPath, json);
    AnsiConsole.MarkupLine("[green]✓ OpenAI API key has been configured successfully.[/]");
    return 0;
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]✗ Failed to save secrets: {ex.Message}[/]");
    return 1;
}

static string GetSecretsFilePath(string userSecretsId)
{
    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var secretsPath = Path.Combine(
        appData,
        "Microsoft",
        "UserSecrets",
        userSecretsId,
        "secrets.json"
    );
    return secretsPath;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string?>), TypeInfoPropertyName = "StringDictionary")]
internal sealed partial class SecretsJsonContext : JsonSerializerContext;
