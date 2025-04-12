#!/bin/bash

# Exit on error
set -e

echo "🚀 Starting k3d deployment..."

# Check if cluster exists
if k3d cluster list | grep -q "bookworm"; then
    echo "⚠️  Cluster 'bookworm' already exists. Deleting..."
    k3d cluster delete bookworm
fi

# Create k3d cluster
echo "📦 Creating k3d cluster..."
k3d cluster create bookworm \
    --api-port 6550 \
    --port 80:80@loadbalancer \
    --port 443:443@loadbalancer \
    --agents 2 \
    --k3s-arg "--disable=traefik@server:0" \
    --k3s-arg "--disable=metrics-server@server:0"

# Verify cluster
echo "🔍 Verifying cluster..."
kubectl cluster-info
kubectl get nodes

# Create namespace
echo "📁 Creating namespace..."
kubectl create namespace bookworm --dry-run=client -o yaml | kubectl apply -f -

# Add Helm repository
echo "📦 Adding Helm repository..."
helm repo add aspire https://charts.aspire.com
helm repo update

# Deploy application
echo "🚀 Deploying application..."
helm upgrade --install bookworm \
    ../../deploys/helm \
    --namespace bookworm \
    --create-namespace \
    --wait

echo "✅ Deployment completed successfully!"
echo "📊 Check deployment status:"
kubectl get all -n bookworm
