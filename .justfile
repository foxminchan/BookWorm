# BookWorm Project Justfile
# Cross-platform task runner for the BookWorm application
# Set shell for different platforms

set windows-shell := ["pwsh.exe", "-NoLogo", "-ExecutionPolicy", "Bypass", "-Command"]
set shell := ["bash", "-c"]

# Variables

solution := "BookWorm.slnx"

# Default recipe when running just 'just'

default: run

# Show available recipes

help:
    echo Available recipes:
    echo " restore - Restore NuGet packages and tools"
    echo " features - Enable features for Aspire"
    echo " build - Build the solution"
    echo " test - Run all tests"
    echo " format - Format C# code"
    echo " trust - Trust the development certificate"
    echo " hook - Setup pre-commit hooks"
    echo " clean - Clean build artifacts"
    echo " run - Run the application (default)"
    echo " update-eventcatalog - Update EventCatalog bun packages"
    echo " update-vuepress - Update VuePress bun packages"
    echo " update-k6 - Update K6 bun packages"
    echo " update-keycloakify - Update Keycloakify bun packages"
    echo " update - Update all bun packages (eventcatalog, vuepress, k6, keycloakify)"

# Restore NuGet packages and tools

restore:
    dotnet restore
    dotnet tool restore

# Enable features for Aspire

features: restore
    dotnet aspire config set features.execCommandEnabled true
    dotnet aspire config set features.deployCommandEnabled true

# Build the solution

build: features
    dotnet build {{ solution }}

# Run tests

test: build
    dotnet test {{ solution }}

# Format C# code

format:
    dotnet csharpier format .

# Clean build artifacts

clean:
    dotnet clean {{ solution }}

# Trust the development certificate

trust:
    dotnet dev-certs https --trust

# Setup pre-commit hooks

hook:
    echo "Setting up pre-commit hooks..."
    just _hook-{{ os() }}

# Setup pre-commit hooks (Windows)

_hook-windows:
    git add .husky/pre-commit || echo "Warning: .husky/pre-commit not found"
    echo "Pre-commit hook setup complete"

# Setup pre-commit hooks (Linux/macOS)

_hook-linux _hook-macos:
    git add .husky/pre-commit || echo "Warning: .husky/pre-commit not found"

# Run the application

run: restore trust hook
    dotnet aspire run

# Update EventCatalog bun packages

update-eventcatalog:
    echo "Updating EventCatalog bun packages..."
    just _update-eventcatalog-{{ os() }}

# Update EventCatalog packages (Windows)

_update-eventcatalog-windows:
    cd docs/eventcatalog && bun update

# Update EventCatalog packages (Linux/macOS)

_update-eventcatalog-linux _update-eventcatalog-macos:
    cd docs/eventcatalog && bun update

# Update VuePress bun packages

update-vuepress:
    echo "Updating VuePress bun packages..."
    just _update-vuepress-{{ os() }}

# Update VuePress packages (Windows)

_update-vuepress-windows:
    cd docs/vuepress && bun run docs:update-package

# Update VuePress packages (Linux/macOS)

_update-vuepress-linux _update-vuepress-macos:
    cd docs/vuepress && bun run docs:update-package

# Update K6 bun packages

update-k6:
    echo "Updating K6 bun packages..."
    just _update-k6-{{ os() }}

# Update K6 packages (Windows)

_update-k6-windows:
    cd src/Aspire/BookWorm.AppHost/Container/k6 && bun update

# Update K6 packages (Linux/macOS)

_update-k6-linux _update-k6-macos:
    cd src/Aspire/BookWorm.AppHost/Container/k6 && bun update

# Update Keycloakify bun packages

update-keycloakify:
    echo "Updating Keycloakify bun packages..."
    just _update-keycloakify-{{ os() }}

# Update Keycloakify packages (Windows)

_update-keycloakify-windows:
    cd src/Aspire/BookWorm.AppHost/Container/keycloak/keycloakify && bun update

# Update Keycloakify packages (Linux/macOS)

_update-keycloakify-linux _update-keycloakify-macos:
    cd src/Aspire/BookWorm.AppHost/Container/keycloak/keycloakify && bun update

# Update all bun packages

update: update-eventcatalog update-vuepress update-k6 update-keycloakify
    echo "All bun packages updated successfully!"
