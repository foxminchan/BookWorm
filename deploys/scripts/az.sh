#!/bin/bash

# Exit on error
set -e

echo "🚀 Starting Azure deployment..."

# Login to Azure
echo "🔐 Logging into Azure..."
az login

# Set subscription if provided
if [ ! -z "$1" ]; then
    echo "📋 Setting subscription to $1..."
    az account set --subscription "$1"
fi

# Generate random password for PostgreSQL
POSTGRES_PASSWORD=$(openssl rand -base64 32)

echo "📦 Deploying infrastructure..."
az deployment sub create \
    --name BookWormDeployment \
    --location eastus \
    --template-file ../bicep/main.bicep \
    --parameters \
        environmentName=dev \
        location=eastus \
        principalId=$(az ad signed-in-user show --query id -o tsv) \
        postgres_password=$POSTGRES_PASSWORD

echo "✅ Deployment completed successfully!"
echo "📊 Check deployment status:"
az deployment sub show --name BookWormDeployment
