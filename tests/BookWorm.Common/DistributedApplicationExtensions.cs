using System.Security.Cryptography;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

namespace BookWorm.Common;

public static class DistributedApplicationExtensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IDistributedApplicationTestingBuilder
    {
        public TBuilder WithRandomParameterValues()
        {
            var parameters = builder
                .Resources.OfType<ParameterResource>()
                .Where(p => !p.IsConnectionString)
                .ToList();

            foreach (var parameter in parameters)
            {
                builder.Configuration[$"Parameters:{parameter.Name}"] = parameter.Secret
                    ? PasswordGenerator.Generate(16, true, true, true, false, 1, 1, 1, 0)
                    : Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
            }

            return builder;
        }

        public TBuilder WithContainersLifetime(ContainerLifetime containerLifetime)
        {
            var containerLifetimeAnnotations = builder
                .Resources.SelectMany(r =>
                    r.Annotations.OfType<ContainerLifetimeAnnotation>()
                        .Where(c => c.Lifetime != containerLifetime)
                )
                .ToList();

            foreach (var annotation in containerLifetimeAnnotations)
            {
                annotation.Lifetime = containerLifetime;
            }

            return builder;
        }

        public TBuilder WithRandomVolumeNames()
        {
            var allResourceNamedVolumes = builder
                .Resources.SelectMany(r =>
                    r.Annotations.OfType<ContainerMountAnnotation>()
                        .Where(m =>
                            m.Type == ContainerMountType.Volume && !string.IsNullOrEmpty(m.Source)
                        )
                        .Select(m => (Resource: r, Volume: m))
                )
                .ToList();

            var seenVolumes = new HashSet<string>();

            var renamedVolumes = new Dictionary<string, string>();

            foreach (
                var name in allResourceNamedVolumes
                    .Select(resourceVolume => resourceVolume.Volume.Source!)
                    .Where(name => !seenVolumes.Add(name) && !renamedVolumes.ContainsKey(name))
            )
            {
                renamedVolumes[name] =
                    $"{name}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(4))}";
            }

            // Replace all named volumes with randomly named or anonymous volumes
            foreach (var (resource, volume) in allResourceNamedVolumes)
            {
                var newName = renamedVolumes.GetValueOrDefault(volume.Source!);
                var newMount = new ContainerMountAnnotation(
                    newName,
                    volume.Target,
                    ContainerMountType.Volume,
                    volume.IsReadOnly
                );
                resource.Annotations.Remove(volume);
                resource.Annotations.Add(newMount);
            }

            return builder;
        }

        public TBuilder WithIncludeResources(params List<string> resourceNames)
        {
            if (resourceNames.Count == 0)
            {
                return builder;
            }

            // Track explicitly requested resources before auto-discovery.
            var explicitResourceNames = new HashSet<string>(resourceNames);

            int added;
            do
            {
                // Find resources that have a "Parent" relationship annotation
                // pointing to a resource already in the include list,
                // but are not yet in the list themselves.
                var annotations = builder
                    .Resources.Where(r =>
                        !resourceNames.Contains(r.Name)
                        && r.Annotations.OfType<ResourceRelationshipAnnotation>()
                            .Any(p => resourceNames.Contains(p.Resource.Name) && p.Type == "Parent")
                    )
                    .Select(r => r.Name);

                // Find the parents of resources already in the include list
                // that are not yet in the list themselves.
                var parents = builder
                    .Resources.OfType<IResourceWithParent>()
                    .Where(r =>
                        resourceNames.Contains(r.Name) && !resourceNames.Contains(r.Parent.Name)
                    )
                    .Select(r => r.Parent.Name);

                List<string> adds = [.. annotations, .. parents];
                resourceNames.AddRange(adds);

                added = adds.Count;
            } while (added > 0);

            var includedNames = resourceNames.Distinct().ToHashSet();

            foreach (
                var resource in builder
                    .Resources.Where(r => !includedNames.Contains(r.Name))
                    .ToArray()
            )
            {
                builder.Resources.Remove(resource);
            }

            // Build a set of resource names that can produce lifecycle events
            // (state transitions like Starting → Running → Healthy).
            // Only containers, projects, and executables have lifecycle in Aspire's
            // orchestrator. Child resources like database or blob container resources
            // derive their state from their parent container, so waiting for them
            // directly is unnecessary — the parent container wait covers them.
            var lifecycleResourceNames = builder
                .Resources.Where(r =>
                    r is ProjectResource or ExecutableResource
                    || r.Annotations.OfType<ContainerImageAnnotation>().Any()
                )
                .Select(r => r.Name)
                .ToHashSet();

            // Remove WaitAnnotations targeting resources that cannot produce
            // lifecycle events. This handles:
            // 1. Resources removed from the model (excluded services like keycloak)
            // 2. Auto-discovered Azure provisioning artifacts (e.g., Key Vault)
            // 3. Connection-string-only child resources (e.g., blob containers,
            //    database children) that never transition to Running/Healthy
            //
            // This is safe because Aspire's WaitForCore recursively adds
            // WaitAnnotations for parent chains, so the actual container/emulator
            // parent is already waited on.
            foreach (var resource in builder.Resources)
            {
                var orphanedWaits = resource
                    .Annotations.OfType<WaitAnnotation>()
                    .Where(w => !lifecycleResourceNames.Contains(w.Resource.Name))
                    .ToList();

                foreach (var wait in orphanedWaits)
                {
                    resource.Annotations.Remove(wait);
                }

                var orphanedRelationships = resource
                    .Annotations.OfType<ResourceRelationshipAnnotation>()
                    .Where(r => !includedNames.Contains(r.Resource.Name))
                    .ToList();

                foreach (var relationship in orphanedRelationships)
                {
                    resource.Annotations.Remove(relationship);
                }
            }

            // Clean up environment variable references to removed resources.
            // When resources are excluded from the model (e.g., chat, embedding,
            // keycloak), their endpoint and connection string references in
            // remaining resources' environment callbacks become unresolvable.
            // The DCP blocks indefinitely waiting for EndpointReference values
            // that will never be allocated. This adds a final callback that
            // handles those unresolvable values after all original callbacks
            // have populated the environment variables dictionary.
            //
            // EndpointReferences (service discovery) are removed entirely since
            // they're only needed for inter-service communication.
            // ConnectionStringReferences are replaced with a stub value so that
            // client libraries (e.g., OpenAI) still register their DI services
            // — the service process starts but AI calls fail gracefully at runtime.
            foreach (var resource in builder.Resources.OfType<IResourceWithEnvironment>())
            {
                resource.Annotations.Add(
                    new EnvironmentCallbackAnnotation(context =>
                    {
                        var keysToRemove = new List<string>();
                        var keysToStub = new List<string>();

                        foreach (var kvp in context.EnvironmentVariables)
                        {
                            switch (kvp.Value)
                            {
                                case EndpointReference er
                                    when !includedNames.Contains(er.Resource.Name):
                                    keysToRemove.Add(kvp.Key);
                                    break;
                                case ConnectionStringReference csr
                                    when !includedNames.Contains(csr.Resource.Name):
                                    keysToStub.Add(kvp.Key);
                                    break;
                            }
                        }

                        foreach (var key in keysToRemove)
                        {
                            context.EnvironmentVariables.Remove(key);
                        }

                        // Stub connection strings with a dummy value so
                        // client libraries can register DI services without
                        // blocking on the unresolvable resource endpoint.
                        foreach (var key in keysToStub)
                        {
                            context.EnvironmentVariables[key] =
                                "Endpoint=https://not-configured.test;Key=not-configured;Model=not-configured";
                        }

                        return Task.CompletedTask;
                    })
                );
            }

            // Remove auto-discovered resources that have no runnable lifecycle.
            // These are Azure provisioning artifacts (e.g., Key Vault resources)
            // that should not appear in the test resource model.
            // Explicitly requested resources are kept even without lifecycle
            // since they provide connection strings or other configuration.
            foreach (
                var resource in builder
                    .Resources.Where(r =>
                        !lifecycleResourceNames.Contains(r.Name)
                        && !explicitResourceNames.Contains(r.Name)
                    )
                    .ToArray()
            )
            {
                builder.Resources.Remove(resource);
            }

            return builder;
        }
    }
}
