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

            // Remove orphaned WaitAnnotations and ResourceRelationshipAnnotations
            // that reference resources no longer in the model, otherwise resources
            // will be stuck in "Waiting" state forever.
            //
            // WaitAnnotations: only keep waits for explicitly requested resources.
            // Auto-discovered parents/children may be Azure provisioning artifacts
            // (e.g., Key Vault resources from WithPasswordAuthentication) that have
            // no runnable lifecycle. Aspire's WaitForCore also recursively adds
            // WaitAnnotations for parent chains, which would reference these
            // non-runnable resources and cause indefinite blocking.
            foreach (var resource in builder.Resources)
            {
                var orphanedWaits = resource
                    .Annotations.OfType<WaitAnnotation>()
                    .Where(w => !explicitResourceNames.Contains(w.Resource.Name))
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

            // Remove auto-discovered resources that have no runnable lifecycle.
            // These are Azure provisioning artifacts (e.g., Key Vault resources)
            // that were auto-included for model integrity but cannot start or
            // become healthy, causing the test framework to time out.
            foreach (
                var resource in builder
                    .Resources.Where(r => !explicitResourceNames.Contains(r.Name))
                    .ToArray()
            )
            {
                var hasLifecycle =
                    resource is ProjectResource or ExecutableResource
                    || resource.Annotations.OfType<ContainerImageAnnotation>().Any()
                    || resource.Annotations.OfType<EndpointAnnotation>().Any()
                    || resource.Annotations.OfType<ContainerLifetimeAnnotation>().Any();

                if (!hasLifecycle)
                {
                    builder.Resources.Remove(resource);
                }
            }

            return builder;
        }
    }
}
