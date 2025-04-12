#!/bin/bash

# Exit on error
set -e

echo "🧹 Starting k3d cleanup..."

# Delete Helm release
echo "🗑️  Deleting Helm release..."
helm uninstall bookworm -n bookworm || true

# Delete namespace
echo "🗑️  Deleting namespace..."
kubectl delete namespace bookworm || true

# Delete k3d cluster
echo "🗑️  Deleting k3d cluster..."
k3d cluster delete bookworm || true

echo "✅ Cleanup completed successfully!"
echo "📊 Check cleanup status:"
k3d cluster list
