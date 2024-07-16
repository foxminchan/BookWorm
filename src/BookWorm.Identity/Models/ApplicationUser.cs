// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Identity;

namespace BookWorm.Identity.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public virtual string? FirstName { get; set; }

    [PersonalData]
    public virtual string? LastName { get; set; }
}
