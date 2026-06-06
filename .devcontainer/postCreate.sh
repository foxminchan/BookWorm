#!/bin/bash

echo "🚀 Setting up BookWorm development environment..."

sudo apt-get update && \
    sudo apt upgrade -y &&
    sudo apt clean -y &&

echo "Installing tools via mise..."
mise install

echo "Install Aspire"
curl -sSL https://aspire.dev/install.sh | bash

echo "Installing Buf CLI"
bun install -g @bufbuild/buf --ignore-scripts

echo "✅ BookWorm development environment setup complete!"
