# Check if OpenAI API Key is set in AppHost secrets
# If not set, prompt user for input and set it

$projectPath = "src/Aspire/BookWorm.AppHost/BookWorm.AppHost.csproj"
$secretKey = "Parameters:openai-openai-apikey"

Write-Host "Checking for OpenAI API key in user secrets..." -ForegroundColor Cyan

# Get current secrets
$secrets = dotnet user-secrets list --project $projectPath 2>&1 | Out-String

# Check if the OpenAI API key is set
if ($secrets -match "$secretKey\s*=\s*.+") {
    Write-Host "✓ OpenAI API key is already configured." -ForegroundColor Green
    exit 0
}

Write-Host "⚠ OpenAI API key is not configured." -ForegroundColor Yellow
Write-Host ""
Write-Host "Please enter your OpenAI API key (or press Ctrl+C to cancel):" -ForegroundColor Cyan
Write-Host "You can get your API key from: https://platform.openai.com/api-keys" -ForegroundColor Gray
Write-Host ""

$apiKey = Read-Host -Prompt "OpenAI API Key" -AsSecureString

# Convert SecureString to plain text for validation
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($apiKey)
$plainKey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
[System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)

if ([string]::IsNullOrWhiteSpace($plainKey)) {
    Write-Host ""
    Write-Host "✗ No API key provided. Cannot proceed." -ForegroundColor Red
    Write-Host "Please set the OpenAI API key and try again." -ForegroundColor Yellow
    exit 1
}

# Set the secret
Write-Host ""
Write-Host "Setting OpenAI API key..." -ForegroundColor Cyan
dotnet user-secrets set $secretKey $plainKey --project $projectPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ OpenAI API key has been configured successfully." -ForegroundColor Green
    # Clear the plain text key from memory
    $plainKey = $null
    exit 0
}
else {
    Write-Host "✗ Failed to set OpenAI API key." -ForegroundColor Red
    exit 1
}
