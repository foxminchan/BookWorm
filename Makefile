.PHONY: restore run setup-secrets

# Default target when running just 'make'
default: run

# Restore NuGet packages and tools
restore:
    dotnet restore
    dotnet tool restore

# Run the application
run: restore
    cd src/BookWorm.AppHost && \
    dotnet run --project BookWorm.AppHost.csproj
