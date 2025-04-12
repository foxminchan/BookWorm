# Deployment Guide

This guide provides detailed instructions for deploying the BookWorm application to different environments.

## Table of Contents

- [Deployment Guide](#deployment-guide)
  - [Table of Contents](#table-of-contents)
  - [Prerequisites](#prerequisites)
  - [Deployment Options](#deployment-options)
    - [Azure Kubernetes Service (AKS)](#azure-kubernetes-service-aks)
      - [Deployment Steps](#deployment-steps)
      - [Cleanup](#cleanup)
    - [k3d (Local Kubernetes)](#k3d-local-kubernetes)
      - [Deployment Steps](#deployment-steps-1)
      - [Cleanup](#cleanup-1)
  - [Deployment Process](#deployment-process)
  - [Monitoring](#monitoring)
    - [AKS Monitoring](#aks-monitoring)
    - [k3d Monitoring](#k3d-monitoring)
  - [Troubleshooting](#troubleshooting)
    - [Common Issues](#common-issues)
    - [Debugging Steps](#debugging-steps)
  - [Cleanup](#cleanup-2)
    - [AKS Cleanup](#aks-cleanup)
    - [k3d Cleanup](#k3d-cleanup)
  - [Contributing](#contributing)

## Prerequisites

Before deploying, ensure you have the following tools installed:

| Tool          | Purpose                       | Installation Guide                                                                  |
| ------------- | ----------------------------- | ----------------------------------------------------------------------------------- |
| Azure CLI     | AKS deployment and management | [Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)       |
| k3d           | Local Kubernetes cluster      | [Install Guide](https://k3d.io/)                                                    |
| kubectl       | Kubernetes management         | [Install Guide](https://kubernetes.io/docs/tasks/tools/)                            |
| Helm          | Package management            | [Install Guide](https://helm.sh/docs/intro/install/)                                |
| Docker/Podman | Container runtime             | [Docker](https://www.docker.com/get-started) / [Podman](https://podman-desktop.io/) |

## Deployment Options

### Azure Kubernetes Service (AKS)

Best suited for production environments, AKS provides:

- Managed Kubernetes service
- Auto-scaling capabilities
- High availability
- Azure service integration

#### Deployment Steps

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
   # Get AKS credentials
   az aks get-credentials --resource-group rg-dev --name bookworm-aks

   # Check deployment status
   kubectl get all -n bookworm
   ```

#### Cleanup

```bash
bash ./scripts/az-cleanup.sh
```

### k3d (Local Kubernetes)

Ideal for development and testing:

- Runs locally
- Lightweight and fast
- Perfect for CI/CD
- Easy to reset

#### Deployment Steps

1. **Deploy to k3d**

   ```bash
   bash ./scripts/k3d.sh
   ```

2. **Verify Deployment**

   ```bash
   # Check cluster status
   k3d cluster list

   # Check deployment status
   kubectl get all -n bookworm
   ```

#### Cleanup

```bash
bash ./scripts/k3d-cleanup.sh
```

## Deployment Process

Both deployment methods follow these steps:

1. **Cluster Setup**

   - Create Kubernetes cluster
   - Configure networking
   - Set up storage

2. **Namespace Creation**

   - Create `bookworm` namespace
   - Set up RBAC

3. **Application Deployment**

   - Deploy using Helm
   - Configure services
   - Set up ingress

4. **Service Configuration**

   - Configure databases
   - Set up messaging
   - Configure monitoring

5. **Monitoring Setup**
   - Configure logging
   - Set up metrics
   - Configure alerts

## Monitoring

### AKS Monitoring

```bash
# View cluster metrics
kubectl top nodes

# View pod metrics
kubectl top pods -n bookworm

# View logs
kubectl logs -n bookworm <pod-name>
```

### k3d Monitoring

```bash
# View cluster status
k3d cluster list

# View resources
kubectl get all -n bookworm

# View logs
kubectl logs -n bookworm <pod-name>
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
   # Check resource usage
   kubectl top nodes
   kubectl top pods -n bookworm
   ```

3. **Network Problems**
   ```bash
   # Check network connectivity
   kubectl get svc -n bookworm
   ```

### Debugging Steps

1. **Check Pod Status**

   ```bash
   kubectl describe pod -n bookworm <pod-name>
   ```

2. **View Deployment Status**

   ```bash
   kubectl describe deployment -n bookworm
   ```

3. **Check Events**
   ```bash
   kubectl get events -n bookworm
   ```

## Cleanup

### AKS Cleanup

```bash
# Run cleanup script
bash ./scripts/az-cleanup.sh

# Verify cleanup
az group list --query "[?name=='rg-dev']" -o table
```

### k3d Cleanup

```bash
# Run cleanup script
bash ./scripts/k3d-cleanup.sh

# Verify cleanup
k3d cluster list
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
