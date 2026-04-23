# BookWorm — Architecture boundary guard pre-tool hook
# Enforces microservice boundaries by preventing cross-service internal references.
$ErrorActionPreference = 'Stop'

$RawInput = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($RawInput)) { exit 0 }
try { $Data = $RawInput | ConvertFrom-Json } catch { exit 0 }
$ToolName = $Data.toolName
$ToolArgs = $Data.toolArgs

# Only check file creation/edits in service directories
if ($ToolName -ne 'edit' -and $ToolName -ne 'create') {
    exit 0
}

$FilePath = if ($ToolArgs.path) { $ToolArgs.path } elseif ($ToolArgs.filePath) { $ToolArgs.filePath } else { '' }
$Content = if ($ToolArgs.content) { $ToolArgs.content }
elseif ($ToolArgs.newText) { $ToolArgs.newText }
elseif ($ToolArgs.new_string) { $ToolArgs.new_string }
else { '' }

# Service names in the project
$Services = @('Catalog', 'Basket', 'Ordering', 'Rating', 'Chat', 'Finance', 'Notification', 'Scheduler', 'McpTools')

# Determine which service this file belongs to
$CurrentService = ''
foreach ($svc in $Services) {
    if ($FilePath -match "Services/$svc/") {
        $CurrentService = $svc
        break
    }
}

# If not in a service directory, allow
if (-not $CurrentService) {
    exit 0
}

# Check for direct references to other services' internal namespaces
foreach ($svc in $Services) {
    if ($svc -eq $CurrentService) { continue }

    # Check for using statements or direct namespace references to other services
    if ($Content -match "using\s+BookWorm\.$svc\.(Domain|Infrastructure|Features|Grpc)") {
        @{
            permissionDecision       = 'deny'
            permissionDecisionReason = "Cross-service boundary violation: $CurrentService service must not directly reference $svc internal namespaces. Use integration events (MassTransit), gRPC contracts, or SharedKernel instead."
        } | ConvertTo-Json -Compress
        exit 0
    }

    # Check for direct project references to other services
    if ($Content -match "ProjectReference.*BookWorm\.$svc[/\\]") {
        @{
            permissionDecision       = 'deny'
            permissionDecisionReason = "Cross-service boundary violation: $CurrentService cannot have a direct ProjectReference to $svc. Services communicate via messaging (MassTransit/Kafka) or gRPC."
        } | ConvertTo-Json -Compress
        exit 0
    }
}

# Allow
exit 0
