#!/bin/bash

set -e # Exit immediately if a command exits with a non-zero status

# Check if bicep is installed (az bicep version)
if ! az bicep version &> /dev/null; then
  az bicep install
fi

# Set initial variables
RESOURCE_GROUP="bookworm-rg"
LOCATION="southeastasia"
ENVIRONMENT_NAME="prod"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
  key="$1"
  case $key in
    --resource-group|-g)
      RESOURCE_GROUP="$2"
      shift 2
      ;;
    --location|-l)
      LOCATION="$2"
      shift 2
      ;;
    --environment|-e)
      ENVIRONMENT_NAME="$2"
      shift 2
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

echo "📦 Deploying BookWorm to Azure..."
echo "Resource Group: $RESOURCE_GROUP"
echo "Location: $LOCATION"
echo "Environment: $ENVIRONMENT_NAME"

# Create resource group if it doesn't exist
echo "Creating resource group if it doesn't exist..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Deploy Bicep template
echo "Deploying infrastructure using Bicep..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file ../bicep/main.bicep \
  --parameters environmentName=$ENVIRONMENT_NAME location=$LOCATION \
  --output json)

# Extract output values from the deployment
AKS_CLUSTER_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.aksClusterName.value')
RESOURCE_GROUP_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.resourceGroupName.value')

echo "Deployment completed successfully!"
echo "AKS Cluster: $AKS_CLUSTER_NAME"

# Get AKS credentials
echo "Getting AKS credentials..."
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER_NAME --overwrite-existing

# Deploy application using Aspire
echo "Deploying BookWorm application to AKS..."
cd ../../
dotnet tool restore
dotnet aspire apply -k $AKS_CLUSTER_NAME --non-interactive -p ./src/BookWorm.AppHost

echo "🚀 BookWorm deployment completed successfully!"
echo "You can access your AKS cluster with: kubectl config use-context $AKS_CLUSTER_NAME"
