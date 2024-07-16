// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace BookWorm.Identity.Pages.Grants;

public sealed class ViewModel
{
    public IEnumerable<GrantViewModel> Grants { get; set; } = [];
}

public sealed class GrantViewModel
{
    public string? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientUrl { get; set; }
    public string? ClientLogoUrl { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Expires { get; set; }
    public IEnumerable<string> IdentityGrantNames { get; set; } = [];
    public IEnumerable<string> ApiGrantNames { get; set; } = [];
}
