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

            foreach (
                var resource in builder
                    .Resources.Where(r => !resourceNames.Distinct().Contains(r.Name))
                    .ToArray()
            )
            {
                builder.Resources.Remove(resource);
            }

            return builder;
        }
    }
}
