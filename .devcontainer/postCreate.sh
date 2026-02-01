#!/bin/bash

echo "🚀 Setting up BookWorm development environment..."

sudo apt-get update && \
    sudo apt upgrade -y &&
    sudo apt clean -y &&

echo Install Aspire
curl -sSL https://aspire.dev/install.sh | bash

echo Installing pnpm
curl -fsSL https://get.pnpm.io/install.sh | sh -

echo "Installing just-cli globally"
cargo install just

echo "Installing Buf CLI"
curl -sSL https://github.com/bufbuild/buf/releases/latest/download/buf-Linux-x86_64 -o /usr/local/bin/buf && chmod +x /usr/local/bin/buf

echo "✅ BookWorm development environment setup complete!"
