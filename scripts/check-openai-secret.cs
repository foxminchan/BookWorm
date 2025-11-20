#!/usr/bin/env dotnet

#:property ManagePackageVersionsCentrally=false
#:package Spectre.Console@0.54.0
#:package Microsoft.Extensions.Configuration.UserSecrets@10.0.0
#:package Microsoft.Extensions.Configuration.Abstractions@10.0.0

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

const string AspireSettingsPath = ".aspire/settings.json";
const string SecretKey = "Parameters:openai-openai-apikey";
const string ApiKeyUrl = "https://platform.openai.com/api-keys";

AnsiConsole.MarkupLine("[cyan]Checking for OpenAI API key in user secrets...[/]");

// Get the AppHost project path from Aspire settings
var projectPath = GetAppHostPath(AspireSettingsPath);
if (string.IsNullOrWhiteSpace(projectPath))
{
    AnsiConsole.MarkupLine("[red]✗ Failed to read AppHost path from Aspire settings.[/]");
    return 1;
}

// Get the UserSecretsId from the project file
var userSecretsId = GetUserSecretsId(projectPath);
if (string.IsNullOrWhiteSpace(userSecretsId))
{
    AnsiConsole.MarkupLine("[red]✗ UserSecretsId not found in project file.[/]");
    return 1;
}

// Build configuration to read user secrets
var configuration = new ConfigurationBuilder().AddUserSecrets(userSecretsId).Build();

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
var secretsPath = GetSecretsFilePath(userSecretsId);
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

static string GetUserSecretsId(string projectPath)
{
    try
    {
        var doc = XDocument.Load(projectPath);
        return doc.Descendants("UserSecretsId").FirstOrDefault()?.Value ?? string.Empty;
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]✗ Failed to read project file: {ex.Message}[/]");
        return string.Empty;
    }
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

static string GetAppHostPath(string aspireSettingsPath)
{
    try
    {
        if (!File.Exists(aspireSettingsPath))
        {
            AnsiConsole.MarkupLine(
                $"[yellow]⚠ Aspire settings file not found at: {aspireSettingsPath}[/]"
            );
            return string.Empty;
        }

        var json = File.ReadAllText(aspireSettingsPath);
        var settings = JsonSerializer.Deserialize(
            json,
            AspireSettingsJsonContext.Default.AspireSettings
        );

        if (settings?.AppHostPath is null)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ appHostPath not found in Aspire settings.[/]");
            return string.Empty;
        }

        // Resolve relative path from the Aspire settings directory
        var aspireDir = Path.GetDirectoryName(Path.GetFullPath(aspireSettingsPath)) ?? string.Empty;
        var absolutePath = Path.GetFullPath(Path.Combine(aspireDir, settings.AppHostPath));

        if (!File.Exists(absolutePath))
        {
            AnsiConsole.MarkupLine($"[yellow]⚠ AppHost project not found at: {absolutePath}[/]");
            return string.Empty;
        }

        return absolutePath;
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]✗ Failed to read Aspire settings: {ex.Message}[/]");
        return string.Empty;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string?>), TypeInfoPropertyName = "StringDictionary")]
internal sealed partial class SecretsJsonContext : JsonSerializerContext;

[JsonSerializable(typeof(AspireSettings))]
internal sealed partial class AspireSettingsJsonContext : JsonSerializerContext;

internal sealed class AspireSettings
{
    [JsonPropertyName("appHostPath")]
    public string? AppHostPath { get; set; }
}
