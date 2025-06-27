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
    @echo "Available recipes:"
    @echo "  restore  - Restore NuGet packages and tools"
    @echo "  build    - Build the solution"
    @echo "  test     - Run all tests"
    @echo "  format   - Format C# code"
    @echo "  trust    - Trust the development certificate"
    @echo "  hook     - Setup pre-commit hooks"
    @echo "  clean    - Clean build artifacts"
    @echo "  gpu [value] - Configure GPU support for Ollama (1=enable, 0=disable, default=0)"
    @echo "  run      - Run the application (default)"
    @echo "  update-eventcatalog - Update EventCatalog npm packages"
    @echo "  update-vuepress - Update VuePress npm packages"
    @echo "  update-k6 - Update K6 npm packages"
    @echo "  update   - Update all npm packages (eventcatalog, vuepress, k6)"

# Restore NuGet packages and tools
restore:
    dotnet restore
    dotnet tool restore

# Build the solution
build: restore
    dotnet build {{solution}}

# Run tests
test: build
    dotnet test {{solution}}

# Format C# code
format: build
    dotnet csharpier format .

# Clean build artifacts
clean:
    dotnet clean {{solution}}

# Trust the development certificate
trust:
    dotnet dev-certs https --trust

# Setup pre-commit hooks
hook:
    @echo "Setting up pre-commit hooks..."
    @just _hook-{{os()}}

# Setup pre-commit hooks (Windows)
_hook-windows:
    @if (Test-Path .husky/pre-commit) { git add .husky/pre-commit; Write-Host "Pre-commit hook added to git" } else { Write-Host "Warning: .husky/pre-commit not found" }

# Setup pre-commit hooks (Linux/macOS)
_hook-linux _hook-macos:
    @if [ -f .husky/pre-commit ]; then git add .husky/pre-commit && echo "Pre-commit hook added to git"; else echo "Warning: .husky/pre-commit not found"; fi

# Run the application
run: restore trust hook
    dotnet aspire run

# Enable/Disable GPU support for Ollama
# Usage: just gpu [value]
# Where value is 1 (enable) or 0 (disable)
# If no value provided or invalid value, defaults to 0 (disable)
gpu value="":
    @echo "Configuring GPU support for Ollama..."
    @just _gpu-{{os()}} "{{value}}"

# GPU support configuration (Windows)
_gpu-windows value="":
    @if ("{{value}}" -eq "") { $gpu_value = "0"; Write-Host "No value provided, defaulting to disable (0)" -ForegroundColor Yellow } elseif ("{{value}}" -eq "1") { $gpu_value = "1" } elseif ("{{value}}" -eq "0") { $gpu_value = "0" } else { $gpu_value = "0"; Write-Host "Invalid value '{{value}}', defaulting to disable (0)" -ForegroundColor Yellow }; [Environment]::SetEnvironmentVariable("OLLAMA_USE_GPU", $gpu_value, "Process"); Write-Host "GPU support set to: $gpu_value. Run 'just run' to start the application." -ForegroundColor Green

# GPU support configuration (Linux/macOS)
_gpu-linux _gpu-macos value="":
    @if [ "{{value}}" = "" ]; then gpu_value="0"; echo "No value provided, defaulting to disable (0)"; elif [ "{{value}}" = "1" ]; then gpu_value="1"; elif [ "{{value}}" = "0" ]; then gpu_value="0"; else gpu_value="0"; echo "Invalid value '{{value}}', defaulting to disable (0)"; fi; export OLLAMA_USE_GPU=$gpu_value && echo "GPU support set to: $gpu_value. Run 'just run' to start the application."

# Update EventCatalog npm packages
update-eventcatalog:
    @echo "Updating EventCatalog npm packages..."
    @just _update-eventcatalog-{{os()}}

# Update EventCatalog packages (Windows)
_update-eventcatalog-windows:
    @cd docs/eventcatalog && npm upgrade

# Update EventCatalog packages (Linux/macOS)
_update-eventcatalog-linux _update-eventcatalog-macos:
    @cd docs/eventcatalog && npm upgrade

# Update VuePress npm packages
update-vuepress:
    @echo "Updating VuePress npm packages..."
    @just _update-vuepress-{{os()}}

# Update VuePress packages (Windows)
_update-vuepress-windows:
    @cd docs/vuepress && npm run docs:update-package

# Update VuePress packages (Linux/macOS)
_update-vuepress-linux _update-vuepress-macos:
    @cd docs/vuepress && npm run docs:update-package

# Update K6 npm packages
update-k6:
    @echo "Updating K6 npm packages..."
    @just _update-k6-{{os()}}

# Update K6 packages (Windows)
_update-k6-windows:
    @cd src/Aspire/BookWorm.AppHost/Container/k6 && npm update

# Update K6 packages (Linux/macOS)
_update-k6-linux _update-k6-macos:
    @cd src/Aspire/BookWorm.AppHost/Container/k6 && npm update

# Update all npm packages
update: update-eventcatalog update-vuepress update-k6
    @echo "All npm packages updated successfully!"
