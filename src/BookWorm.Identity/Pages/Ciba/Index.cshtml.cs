// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWorm.Identity.Pages.Ciba;

[AllowAnonymous]
[SecurityHeaders]
public sealed class IndexModel(
    IBackchannelAuthenticationInteractionService backchannelAuthenticationInteractionService,
    ILogger<IndexModel> logger
) : PageModel
{
    public BackchannelUserLoginRequest LoginRequest { get; set; } = default!;

    public async Task<IActionResult> OnGet(string id)
    {
        var result =
            await backchannelAuthenticationInteractionService.GetLoginRequestByInternalIdAsync(id);
        if (result is null)
        {
            logger.InvalidBackchannelLoginId(id);
            return RedirectToPage("/Home/Error/Index");
        }

        LoginRequest = result;

        return Page();
    }
}
