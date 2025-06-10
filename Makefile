.PHONY: restore run trust hook build test clean format help

# Default target when running just 'make'
default: run

# Show available targets
help:
	@echo "Available targets:"
	@echo "  restore  - Restore NuGet packages and tools"
	@echo "  build    - Build the solution"
	@echo "  test     - Run all tests"
	@echo "  format   - Format C# code"
	@echo "  trust    - Trust the development certificate"
	@echo "  hook     - Setup pre-commit hooks"
	@echo "  clean    - Clean build artifacts"
	@echo "  run      - Run the application (default)"

# Restore NuGet packages and tools
restore:
	dotnet restore
	dotnet tool restore

# Build the solution
build: restore
	dotnet build BookWorm.slnx

# Run tests
test: build
	dotnet test BookWorm.slnx

# Format C# code
format: build
	dotnet csharpier format .

# Clean build artifacts
clean:
	dotnet clean BookWorm.slnx

# Trust the development certificate
trust:
	dotnet dev-certs https --trust

# Setup pre-commit hooks
hook:
	@echo "Setting up pre-commit hooks..."
	@if [ -f .husky/pre-commit ]; then \
		git add .husky/pre-commit; \
		echo "Pre-commit hook added to git"; \
	else \
		echo "Warning: .husky/pre-commit not found"; \
	fi

# Run the application
run: restore trust hook
	dotnet aspire run
