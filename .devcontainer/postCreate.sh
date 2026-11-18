#!/usr/bin/env bash

echo "🚀 Setting up BookWorm development environment..."

sudo apt-get update && \
    sudo apt upgrade -y &&
    sudo apt clean -y &&

echo "Installing tools via mise..."
mise install

echo "✅ BookWorm development environment setup complete!"
