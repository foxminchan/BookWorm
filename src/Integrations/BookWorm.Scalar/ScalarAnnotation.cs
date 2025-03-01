namespace BookWorm.Scalar;

internal sealed record ScalarAnnotation(
    string[] DocumentNames,
    string Route,
    EndpointReference EndpointReference
) : IResourceAnnotation;
