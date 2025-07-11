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

if command -v bun >/dev/null 2>&1; then
    echo "✅ bun version: $(bun --version)"
else
    echo "❌ bun not found"
fi

# Install Just task runner
echo "🔧 Installing Just task runner..."
if command -v bun >/dev/null 2>&1; then
    bun install -g rust-just
    echo "✅ Just installed via bun"

    # Verify Just installation
    if command -v just >/dev/null 2>&1; then
        echo "✅ Just version: $(just --version)"
    else
        echo "⚠️ Just installed but not found in PATH. You may need to restart your terminal."
    fi
else
    echo "❌ bun not available, skipping Just installation"
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
if [ -d "eventcatalog" ]; then
    cd eventcatalog
    if [ -f "package.json" ]; then
        if command -v bun >/dev/null 2>&1; then
            bun install --frozen-lockfile
            echo "✅ EventCatalog dependencies installed"
        else
            echo "❌ bun not available, skipping EventCatalog dependencies"
        fi
    fi
    cd ..
fi

# Install Node.js dependencies for k6 tests
echo "📦 Installing k6 test dependencies..."
if [ -d "src/Aspire/BookWorm.AppHost/Container/k6" ]; then
    cd src/Aspire/BookWorm.AppHost/Container/k6
    if [ -f "package.json" ]; then
        if command -v bun >/dev/null 2>&1; then
            bun install --frozen-lockfile
            echo "✅ k6 test dependencies installed"
        else
            echo "❌ bun not available, skipping k6 test dependencies"
        fi
    fi
    cd ../../../../..
fi

# Install Node.js dependencies for docs
echo "📦 Installing documentation dependencies..."
if [ -d "docs/vuepress" ]; then
    cd docs/vuepress
    if [ -f "package.json" ]; then
        if command -v bun >/dev/null 2>&1; then
            bun install --frozen-lockfile
            echo "✅ Documentation dependencies installed"
        else
            echo "❌ bun not available, skipping documentation dependencies"
        fi
    fi
    cd ../..
fi

# Create helpful aliases
echo "🔗 Setting up helpful aliases..."
cat >> ~/.bashrc << 'EOF'

# BookWorm Development Aliases
alias ll='ls -alF'
alias la='ls -A'
alias l='ls -CF'
alias ..='cd ..'
alias ...='cd ../..'

# BookWorm specific aliases
alias bw-run='just run'
alias bw-build='just build'
alias bw-test='just test'
alias bw-format='just format'
alias bw-clean='just clean'
alias bw-restore='just restore'

# Docker aliases
alias d='docker'
alias dc='docker compose'
alias dps='docker ps'
alias di='docker images'

# Git aliases
alias gs='git status'
alias ga='git add'
alias gc='git commit'
alias gp='git push'
alias gl='git log --oneline'

# .NET aliases
alias dr='dotnet run'
alias db='dotnet build'
alias dt='dotnet test'
alias dw='dotnet watch'

# Node.js/bun aliases
alias bi='bun install'
alias br='bun run'
alias bs='bun start'
alias bt='bun test'
alias bb='bun run build'
alias bci='bun install --frozen-lockfile'

# Navigation aliases
alias gotoapi='cd src/Services'
alias gotodocs='cd docs'
alias gotoevent='cd eventcatalog'
alias gotok6='cd src/Aspire/BookWorm.AppHost/Container/k6'
alias gotoaspire='cd src/Aspire'

EOF

# Trust HTTPS certificates
echo "🔒 Trusting HTTPS development certificates..."
if command -v dotnet >/dev/null 2>&1; then
    dotnet dev-certs https --trust 2>/dev/null || echo "⚠️  Could not trust certificates automatically. You may need to run 'dotnet dev-certs https --trust' manually."
fi

# Display useful information
echo ""
echo "✅ BookWorm development environment setup complete!"
echo ""
echo "🚀 Quick Start Commands:"
echo "  just run          - Start the full application"
echo "  just build        - Build all projects"
echo "  just test         - Run all tests"
echo "  just format       - Format code"
echo "  just help         - Show all available commands"
echo ""
echo "📦 Node.js/bun Commands:"
echo "  cd eventcatalog && bun start  - Start EventCatalog dev server"
echo "  cd docs && bun start          - Start documentation dev server"
echo "  bun install                   - Install dependencies"
echo ""
echo "📚 Documentation:"
echo "  docs/            - Architecture documentation"
echo "  eventcatalog/    - Event-driven architecture docs"
echo "  README.md        - Main project documentation"
echo ""
echo "🔗 Important Ports:"
echo "  5000 - Aspire Dashboard"
echo "  5001 - API Gateway"
echo "  8080 - Keycloak"
echo "  8025 - Mailpit (local email)"
echo "  3000 - EventCatalog dev server"
echo ""
echo "💡 Tip: Use the aliases defined in ~/.bashrc for faster development!"
echo "   Type 'source ~/.bashrc' or restart your terminal to enable them."
echo ""
