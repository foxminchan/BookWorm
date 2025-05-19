# Deployment Guide

This guide provides detailed instructions for deploying the BookWorm application to Azure Container Apps (ACA).

## Table of Contents

- [Deployment Guide](#deployment-guide)
  - [Table of Contents](#table-of-contents)
  - [Prerequisites](#prerequisites)
  - [Deployment to Azure Container Apps](#deployment-to-azure-container-apps)
    - [Deployment Steps](#deployment-steps)
    - [Cleanup](#cleanup)
  - [Deployment Process](#deployment-process)
  - [Monitoring](#monitoring)
  - [Troubleshooting](#troubleshooting)
    - [Common Issues](#common-issues)
    - [Debugging Steps](#debugging-steps)
  - [Cleanup](#cleanup-1)
  - [Contributing](#contributing)

## Prerequisites

Before deploying, ensure you have the following tools installed:

| Tool          | Purpose                       | Installation Guide                                                                  |
| ------------- | ----------------------------- | ----------------------------------------------------------------------------------- |
| Azure CLI     | ACA deployment and management | [Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)       |
| Docker/Podman | Container runtime             | [Docker](https://www.docker.com/get-started) / [Podman](https://podman-desktop.io/) |

## Deployment to Azure Container Apps

Azure Container Apps (ACA) provides:

- Managed container service
- Auto-scaling capabilities
- High availability
- Azure service integration

### Deployment Steps

1. **Login to Azure**

   ```bash
   az login
   ```

2. **Deploy Infrastructure**

   ```bash
   bash ./scripts/az.sh
   ```

3. **Verify Deployment**

   ```bash
   # Get ACA credentials
   az containerapp env show --resource-group rg-dev --name bookworm-env

   # Check deployment status
   az containerapp show --resource-group rg-dev --name bookworm
   ```

### Cleanup

```bash
bash ./scripts/az-cleanup.sh
```

## Deployment Process

The ACA deployment follows these steps:

1. **Environment Setup**

   - Create ACA environment
   - Configure networking
   - Set up storage

2. **Application Deployment**

   - Deploy containers
   - Configure services
   - Set up ingress

3. **Service Configuration**

   - Configure databases
   - Set up messaging
   - Configure monitoring

4. **Monitoring Setup**
   - Configure logging
   - Set up metrics
   - Configure alerts

## Monitoring

```bash
# View container app details
az containerapp show --resource-group rg-dev --name bookworm

# View logs
az containerapp logs show --resource-group rg-dev --name bookworm

# View metrics
az containerapp metrics show --resource-group rg-dev --name bookworm
```

## Troubleshooting

### Common Issues

1. **Port Conflicts**

   ```bash
   # Check used ports
   netstat -tuln | grep LISTEN
   ```

2. **Resource Issues**

   ```bash
   # Check resource usage in Azure
   az containerapp metrics show --resource-group rg-dev --name bookworm
   ```

3. **Network Problems**
   ```bash
   # Check network connectivity
   az containerapp ingress show --resource-group rg-dev --name bookworm
   ```

## Cleanup

```bash
bash ./scripts/az-cleanup.sh

# Verify cleanup
az group show --name rg-dev
```

> [!WARNING]
>
> - Cleanup operations are irreversible
> - Backup important data before cleanup
> - Production deployments require proper monitoring

## Contributing

If you encounter any issues or have suggestions for improvement:

1. Check the [troubleshooting guide](#troubleshooting)
2. Create an issue in the repository
3. Submit a pull request with your improvements
