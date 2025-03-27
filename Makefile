.PHONY: restore run setup-secrets

# Default target when running just 'make'
default: run

# Restore NuGet packages and tools
restore:
    dotnet restore
    dotnet tool restore

# Setup user secrets for the application
setup-secrets:
    cd src/BookWorm.AppHost && \
    dotnet user-secrets set "Parameters:sql-user" "postgres" && \
    dotnet user-secrets set "Parameters:sql-password" "postgres"

# Run the application
run: restore setup-secrets
    cd src/BookWorm.AppHost && \
    dotnet run --project BookWorm.AppHost.csproj
