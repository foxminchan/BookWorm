// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Models;

namespace BookWorm.Identity.Pages.Home.Error;

public sealed class ViewModel
{
    public ViewModel()
    {
    }

    public ViewModel(string error)
    {
        Error = new() { Error = error };
    }

    public ErrorMessage? Error { get; set; }
}
