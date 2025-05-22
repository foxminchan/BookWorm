.PHONY: restore run trust hook

# Default target when running just 'make'
default: run

# Restore NuGet packages and tools
restore:
	dotnet restore
	dotnet tool restore

# Trust the development certificate
trust:
	dotnet dev-certs https --trust

# Add the pre-commit hook
hook:
	git add .husky/pre-commit

# Run the application
run: restore trust hook
	cd src/Aspire/BookWorm.AppHost && \
	dotnet run --project BookWorm.AppHost.csproj
