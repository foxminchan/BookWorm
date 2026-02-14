#!/bin/bash

echo "🚀 Setting up BookWorm development environment..."

sudo apt-get update && \
    sudo apt upgrade -y &&
    sudo apt clean -y &&

echo Install Aspire
curl -sSL https://aspire.dev/install.sh | bash

echo Installing Bun
curl -fsSL --proto '=https' --tlsv1.2 -o /tmp/bun-install.sh https://bun.sh/install && bash /tmp/bun-install.sh && rm /tmp/bun-install.sh

echo "Installing just-cli globally using bun"
bun install -g rust-just

echo "Installing Buf CLI"
bun install -g @bufbuild/buf

echo "✅ BookWorm development environment setup complete!"
