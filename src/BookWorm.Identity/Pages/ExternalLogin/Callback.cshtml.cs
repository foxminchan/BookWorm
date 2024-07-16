// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using BookWorm.Identity.Models;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWorm.Identity.Pages.ExternalLogin;

[AllowAnonymous]
[SecurityHeaders]
public class Callback(
    IIdentityServerInteractionService interaction,
    IEventService events,
    ILogger<Callback> logger,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        // read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result.Succeeded != true)
        {
            throw new InvalidOperationException($"External authentication error: {result.Failure}");
        }

        var externalUser = result.Principal ??
                           throw new InvalidOperationException("External authentication produced a null Principal");

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
            logger.ExternalClaims(externalClaims);
        }

        // lookup our user and external provider info
        // try to determine the unique id of the external user (issued by the provider)
        // the most common claim type for that are the sub-claim and the NameIdentifier
        // depending on the external provider, some other claim type might be used
        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new InvalidOperationException("Unknown userid");

        var provider = result.Properties.Items["scheme"] ??
                       throw new InvalidOperationException("Null scheme in authentication properties");
        var providerUserId = userIdClaim.Value;

        // find external user
        // this might be where you might initiate a custom workflow for user registration
        // in this sample we don't show how that would be done, as our sample implementation
        // simply auto-provisions new external user
        var user = await userManager.FindByLoginAsync(provider, providerUserId) ?? await AutoProvisionUserAsync(provider, providerUserId, externalUser.Claims);

        // this allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // this is typically used to store data needed for sign-out from those protocols.
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

        // issue authentication cookie for user
        await signInManager.SignInWithClaimsAsync(user, localSignInProps, additionalLocalClaims);

        // delete temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // retrieve return URL
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // check if external login is in the context of an OIDC request
        var context = await interaction.GetAuthorizationContextAsync(returnUrl);
        await events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, user.UserName, true,
            context?.Client.ClientId));
        Telemetry.Metrics.UserLogin(context?.Client.ClientId, provider);

        if (context is null)
        {
            return Redirect(returnUrl);
        }

        return context.IsNativeClient() ?
            // The client is native, so this change in how to
            // return the response is for better UX for the end user.
            this.LoadingPage(returnUrl) : Redirect(returnUrl);
    }

    [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection",
        Justification = "<Pending>")]
    private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId,
        IEnumerable<Claim> claims)
    {
        var sub = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = sub,
            UserName = sub // don't need a username, since the user will be using an external provider to login
        };

        // email
        var enumerable = claims as Claim[] ?? claims.ToArray();
        var email = enumerable.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
                    enumerable.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email is not null)
        {
            user.Email = email;
        }

        // create a list of claims that we want to transfer into our store
        var filtered = new List<Claim>();

        // user's display name
        var name = enumerable.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                   enumerable.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if (name is not null)
        {
            filtered.Add(new(JwtClaimTypes.Name, name));
        }
        else
        {
            var first = enumerable.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                        enumerable.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var last = enumerable.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                       enumerable.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            if (first is not null && last is not null)
            {
                filtered.Add(new(JwtClaimTypes.Name, first + " " + last));
            }
            else if (first is not null)
            {
                filtered.Add(new(JwtClaimTypes.Name, first));
            }
            else if (last is not null)
            {
                filtered.Add(new(JwtClaimTypes.Name, last));
            }
        }

        var identityResult = await userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(identityResult.Errors.First().Description);
        }

        if (filtered.Count != 0)
        {
            identityResult = await userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(identityResult.Errors.First().Description);
            }
        }

        identityResult = await userManager.AddLoginAsync(user, new(provider, providerUserId, provider));
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(identityResult.Errors.First().Description);
        }

        return user;
    }

    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private static void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims,
        AuthenticationProperties localSignInProps)
    {
        ArgumentNullException.ThrowIfNull(externalResult.Principal, nameof(externalResult.Principal));

        // capture the idp used to login, so the session knows where the user came from
        localClaims.Add(new(JwtClaimTypes.IdentityProvider,
            externalResult.Properties?.Items["scheme"] ?? "unknown identity provider"));

        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid is not null)
        {
            localClaims.Add(new(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult.Properties?.GetTokenValue("id_token");
        if (idToken is not null)
        {
            localSignInProps.StoreTokens([new() { Name = "id_token", Value = idToken }]);
        }
    }
}
