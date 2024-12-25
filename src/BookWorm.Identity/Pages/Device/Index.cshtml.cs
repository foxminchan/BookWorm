﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using BookWorm.Identity.Pages.Consent;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWorm.Identity.Pages.Device;

[SecurityHeaders]
[Authorize]
public sealed class Index(
    IDeviceFlowInteractionService interaction,
    IEventService eventService) : PageModel
{
    public ViewModel View { get; set; } = default!;

    [BindProperty] public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnGet(string? userCode)
    {
        if (String.IsNullOrWhiteSpace(userCode))
        {
            return Page();
        }

        if (!await SetViewModelAsync(userCode))
        {
            ModelState.AddModelError("", DeviceOptions.InvalidUserCode);
            return Page();
        }

        Input = new() { UserCode = userCode };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var request =
            await interaction.GetAuthorizationContextAsync(Input.UserCode ??
                                                           throw new ArgumentNullException(nameof(Input.UserCode)));
        if (request is null)
        {
            return RedirectToPage("/Home/Error/Index");
        }

        ConsentResponse? grantedConsent = null;

        switch (Input.Button)
        {
            // user clicked 'no' - send back the standard 'access_denied' response
            case "no":
                grantedConsent = new() { Error = AuthorizationError.AccessDenied };

                // emit event
                await eventService.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId,
                    request.ValidatedResources.RawScopeValues));
                Telemetry.Metrics.ConsentDenied(request.Client.ClientId,
                    request.ValidatedResources.ParsedScopes.Select(s => s.ParsedName));
                break;
            // user clicked 'yes' - validate the data
            // if the user consented to some scope, build the response model
            case "yes" when Input.ScopesConsented.Any():
            {
                var scopes = Input.ScopesConsented;
                if (ConsentOptions.EnableOfflineAccess == false)
                {
                    scopes = scopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
                }

                grantedConsent = new()
                {
                    RememberConsent = Input.RememberConsent,
                    ScopesValuesConsented = scopes.ToArray(),
                    Description = Input.Description
                };

                // emit event
                await eventService.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId,
                    request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented,
                    grantedConsent.RememberConsent));
                Telemetry.Metrics.ConsentGranted(request.Client.ClientId, grantedConsent.ScopesValuesConsented,
                    grantedConsent.RememberConsent);
                var denied = request.ValidatedResources.ParsedScopes.Select(s => s.ParsedName)
                    .Except(grantedConsent.ScopesValuesConsented);
                Telemetry.Metrics.ConsentDenied(request.Client.ClientId, denied);
                break;
            }
            case "yes":
                ModelState.AddModelError("", ConsentOptions.MustChooseOneErrorMessage);
                break;
            default:
                ModelState.AddModelError("", ConsentOptions.InvalidSelectionErrorMessage);
                break;
        }

        if (grantedConsent is not null)
        {
            // communicate outcome of consent back to identity-server
            await interaction.HandleRequestAsync(Input.UserCode, grantedConsent);

            // indicate that's it ok to redirect back to authorization endpoint
            return RedirectToPage("/Device/Success");
        }

        // we need to redisplay the consent UI
        if (!await SetViewModelAsync(Input.UserCode))
        {
            return RedirectToPage("/Home/Error/Index");
        }

        return Page();
    }


    private async Task<bool> SetViewModelAsync(string userCode)
    {
        var request = await interaction.GetAuthorizationContextAsync(userCode);
        if (request is not null)
        {
            View = CreateConsentViewModel(request);
            return true;
        }

        View = new();
        return false;
    }

    private ViewModel CreateConsentViewModel(DeviceFlowAuthorizationRequest request)
    {
        var vm = new ViewModel
        {
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent,
            IdentityScopes = request.ValidatedResources.Resources.IdentityResources.Select(x =>
                CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name))).ToArray()
        };

        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope is null)
            {
                continue;
            }

            var scopeVm = CreateScopeViewModel(parsedScope, apiScope,
                Input is null || Input.ScopesConsented.Contains(parsedScope.RawValue));
            apiScopes.Add(scopeVm);
        }

        if (DeviceOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(Input is null ||
                                                Input.ScopesConsented.Contains(IdentityServerConstants.StandardScopes
                                                    .OfflineAccess)));
        }

        vm.ApiScopes = apiScopes;

        return vm;
    }

    private static ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new()
        {
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
    }

    private static ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        return new()
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = apiScope.DisplayName ?? apiScope.Name,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    private static ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new()
        {
            Value = IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = DeviceOptions.OfflineAccessDisplayName,
            Description = DeviceOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
