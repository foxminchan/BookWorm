# PowerShell script for Azure deployment
param(
    [string]$SubscriptionId
)

# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting Azure deployment..." -ForegroundColor Green

# Login to Azure
Write-Host "🔐 Logging into Azure..." -ForegroundColor Cyan
az login

# Set subscription if provided
if ($SubscriptionId) {
    Write-Host "📋 Setting subscription to $SubscriptionId..." -ForegroundColor Cyan
    az account set --subscription $SubscriptionId
}

# Generate random password for PostgreSQL
$RandomBytes = [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes(24)
$PostgresPassword = [Convert]::ToBase64String($RandomBytes)

# Get the current user's principal ID
$PrincipalId = (az ad signed-in-user show --query id -o tsv)

Write-Host "📦 Deploying infrastructure..." -ForegroundColor Cyan
az deployment sub create `
    --name BookWormDeployment `
    --location eastus `
    --template-file ../bicep/main.bicep `
    --parameters `
        environmentName=dev `
        location=eastus `
        principalId=$PrincipalId `
        postgres_password=$PostgresPassword

Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host "📊 Check deployment status:" -ForegroundColor Yellow
az deployment sub show --name BookWormDeployment
