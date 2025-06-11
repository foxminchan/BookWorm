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
