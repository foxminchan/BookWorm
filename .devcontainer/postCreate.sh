#!/bin/bash

# BookWorm DevContainer Post-Create Script
# This script runs after the devcontainer is created to set up the development environment

set -e

echo "🚀 Setting up BookWorm development environment..."

# Update package lists
echo "📦 Updating package lists..."
sudo apt-get update

# Install additional development tools
echo "🔧 Installing additional development tools..."
sudo apt-get install -y \
    curl \
    wget \
    jq \
    tree \
    htop

# Set up Git configuration if not already configured
echo "🔧 Configuring Git..."
if [ -z "$(git config --global user.name)" ]; then
    echo "⚠️ Git user.name not set. You may want to configure it later with:"
    echo "   git config --global user.name 'Your Name'"
fi

if [ -z "$(git config --global user.email)" ]; then
    echo "⚠️ Git user.email not set. You may want to configure it later with:"
    echo "   git config --global user.email 'your.email@example.com'"
fi

# Configure Git to trust the workspace directory
echo "🔒 Configuring Git safe directory..."
git config --global --add safe.directory /workspaces/BookWorm

# Verify Node.js and bun installation
echo "🔧 Verifying Node.js and bun installation..."
if command -v node >/dev/null 2>&1; then
    echo "✅ Node.js version: $(node --version)"
else
    echo "❌ Node.js not found"
fi

# Install bun (JavaScript runtime)
echo "🔧 Installing bun..."
curl -fsSL https://bun.sh/install | bash

# Source bun environment to update PATH
export BUN_INSTALL="$HOME/.bun"
export PATH="$BUN_INSTALL/bin:$PATH"

if command -v bun >/dev/null 2>&1; then
    echo "✅ bun version: $(bun --version)"
else
    echo "❌ bun not found after installation"
fi

# Install Just task runner
echo "🔧 Installing Just task runner..."
if command -v cargo >/dev/null 2>&1; then
    cargo install just
    echo "✅ Just installed via cargo"
else
    # Try installing via apt as fallback
    if sudo apt-get install -y just 2>/dev/null; then
        echo "✅ Just installed via apt"
    else
        echo "❌ Could not install Just. Neither cargo nor apt package available."
    fi
fi

# Verify Just installation
if command -v just >/dev/null 2>&1; then
    echo "✅ Just version: $(just --version)"
else
    echo "⚠️ Just not found in PATH. You may need to restart your terminal or add ~/.cargo/bin to PATH."
fi

# Restore .NET tools and packages
echo "📦 Restoring .NET tools and packages..."
if [ -f "global.json" ]; then
    dotnet tool restore
fi

if [ -f "BookWorm.slnx" ]; then
    dotnet restore BookWorm.slnx
fi

# Set up pre-commit hooks
echo "🪝 Setting up pre-commit hooks..."
if [ -f ".justfile" ]; then
    just hook
fi

# Install Node.js dependencies for EventCatalog
echo "📦 Installing EventCatalog dependencies..."
if [ -d "docs/eventcatalog" ]; then
    pushd docs/eventcatalog > /dev/null
    if [ -f "package.json" ]; then
        if command -v bun >/dev/null 2>&1; then
            bun install --frozen-lockfile
            echo "✅ EventCatalog dependencies installed"
        else
            echo "❌ bun not available, skipping EventCatalog dependencies"
        fi
    fi
    popd > /dev/null
fi

# Install Node.js dependencies for k6 tests
echo "📦 Installing k6 test dependencies..."
if [ -d "src/Aspire/BookWorm.AppHost/Container/k6" ]; then
    pushd src/Aspire/BookWorm.AppHost/Container/k6 > /dev/null
    if [ -f "package.json" ]; then
        if command -v bun >/dev/null 2>&1; then
            bun install --frozen-lockfile
            echo "✅ k6 test dependencies installed"
        else
            echo "❌ bun not available, skipping k6 test dependencies"
        fi
    fi
    popd > /dev/null
fi

# Install Node.js dependencies for docs
echo "📦 Installing documentation dependencies..."
if [ -d "docs/docusaurus" ]; then
    pushd docs/docusaurus > /dev/null
    if [ -f "package.json" ]; then
        if command -v bun >/dev/null 2>&1; then
            bun install --frozen-lockfile
            echo "✅ Documentation dependencies installed"
        else
            echo "❌ bun not available, skipping documentation dependencies"
        fi
    fi
    popd > /dev/null
fi

# Install Spec-Kit (optional)
echo "🔧 Installing Spec-Kit (Optional)..."
if ! command -v uv >/dev/null 2>&1; then
    echo "📦 Installing uv first..."
    curl -LsSf https://astral.sh/uv/install.sh | sh
    export PATH="$HOME/.cargo/bin:$PATH"
fi

if command -v uv >/dev/null 2>&1; then
    uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
    echo "✅ Spec-Kit installed"
else
    echo "❌ uv installation failed, skipping Spec-Kit installation"
fi

# Install GitHub Copilot CLI (optional)
echo "🔧 Installing GitHub Copilot CLI (Optional)..."
if command -v gh >/dev/null 2>&1; then
    bun install -g @github/copilot
    echo "✅ GitHub Copilot CLI installed"
else
    echo "❌ gh not available, skipping GitHub Copilot CLI installation"
fi
