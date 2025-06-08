# PowerShell script for Azure cleanup

# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

Write-Host "🧹 Starting Azure cleanup..." -ForegroundColor Yellow

# Delete resource group
Write-Host "🗑️  Deleting resource group..." -ForegroundColor Red
az group delete --name rg-dev --yes --no-wait

# Delete deployment
Write-Host "🗑️  Deleting deployment..." -ForegroundColor Red
az deployment sub delete --name BookWormDeployment

Write-Host "✅ Cleanup completed successfully!" -ForegroundColor Green
Write-Host "📊 Check cleanup status:" -ForegroundColor Yellow
az group list --query "[?name=='rg-dev']" -o table
