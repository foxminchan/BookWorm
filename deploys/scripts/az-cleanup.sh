#!/bin/bash

# Exit on error
set -e

echo "🧹 Starting Azure cleanup..."

# Delete resource group
echo "🗑️  Deleting resource group..."
az group delete --name rg-dev --yes --no-wait

# Delete deployment
echo "🗑️  Deleting deployment..."
az deployment sub delete --name BookWormDeployment

echo "✅ Cleanup completed successfully!"
echo "📊 Check cleanup status:"
az group list --query "[?name=='rg-dev']" -o table
