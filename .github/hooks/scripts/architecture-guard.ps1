# BookWorm — Architecture boundary guard pre-tool hook
# Enforces microservice boundaries by preventing cross-service internal references.
$ErrorActionPreference = "Stop"

try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $toolName = $hookInput.toolName
    $toolArgs = $hookInput.toolArgs | ConvertFrom-Json -ErrorAction SilentlyContinue

    # Only check file creation/edits in service directories
    if ($toolName -notin @("edit", "create")) {
        exit 0
    }

    $filePath = if ($toolArgs.path) { $toolArgs.path } elseif ($toolArgs.filePath) { $toolArgs.filePath } else { "" }
    $content = if ($toolArgs.content) { $toolArgs.content }
               elseif ($toolArgs.newText) { $toolArgs.newText }
               elseif ($toolArgs.new_string) { $toolArgs.new_string }
               else { "" }

    # Service names in the project
    $services = @("Catalog", "Basket", "Ordering", "Rating", "Chat", "Finance", "Notification", "Scheduler")

    # Determine which service this file belongs to
    $currentService = $null
    foreach ($svc in $services) {
        if ($filePath -like "*Services/$svc/*" -or $filePath -like "*Services\$svc\*") {
            $currentService = $svc
            break
        }
    }

    # If not in a service directory, allow
    if (-not $currentService) {
        exit 0
    }

    # Check for direct references to other services' internal namespaces
    foreach ($svc in $services) {
        if ($svc -eq $currentService) { continue }

        # Check for using statements or direct namespace references to other services
        if ($content -match "using\s+BookWorm\.$svc\.(Domain|Infrastructure|Features|Grpc)") {
            @{
                permissionDecision       = "deny"
                permissionDecisionReason = "Cross-service boundary violation: $currentService service must not directly reference $svc internal namespaces. Use integration events (MassTransit), gRPC contracts, or SharedKernel instead."
            } | ConvertTo-Json -Compress
            exit 0
        }

        # Check for direct project references to other services
        if ($content -match "ProjectReference.*BookWorm\.$svc[/\\]") {
            @{
                permissionDecision       = "deny"
                permissionDecisionReason = "Cross-service boundary violation: $currentService cannot have a direct ProjectReference to $svc. Services communicate via messaging (MassTransit/RabbitMQ) or gRPC."
            } | ConvertTo-Json -Compress
            exit 0
        }
    }

    # Allow
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
