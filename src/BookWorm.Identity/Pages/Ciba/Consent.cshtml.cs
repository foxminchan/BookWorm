// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookWorm.Identity.Pages.Ciba;

[Authorize]
[SecurityHeaders]
public sealed class Consent(
    IBackchannelAuthenticationInteractionService interaction,
    IEventService events,
    ILogger<Consent> logger) : PageModel
{
    public ViewModel View { get; set; } = default!;

    [BindProperty] public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnGet(string? id)
    {
        if (!await SetViewModelAsync(id))
        {
            return RedirectToPage("/Home/Error/Index");
        }

        Input = new() { Id = id };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // validate return url is still valid
        var request =
            await interaction.GetLoginRequestByInternalIdAsync(Input.Id ??
                                                               throw new ArgumentNullException(nameof(Input.Id)));
        if (request is null || request.Subject.GetSubjectId() != User.GetSubjectId())
        {
            logger.InvalidId(Input.Id);
            return RedirectToPage("/Home/Error/Index");
        }

        CompleteBackchannelLoginRequest? result = null;

        switch (Input.Button)
        {
            // user clicked 'no' - send back the standard 'access_denied' response
            case "no":
                result = new(Input.Id);

                // emit event
                await events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId,
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

                result = new(Input.Id) { ScopesValuesConsented = scopes.ToArray(), Description = Input.Description };

                // emit event
                await events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId,
                    request.ValidatedResources.RawScopeValues, result.ScopesValuesConsented, false));
                Telemetry.Metrics.ConsentGranted(request.Client.ClientId, result.ScopesValuesConsented, false);
                var denied = request.ValidatedResources.ParsedScopes.Select(s => s.ParsedName)
                    .Except(result.ScopesValuesConsented);
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

        if (result is not null)
        {
            // communicate outcome of consent back to identity-server
            await interaction.CompleteLoginRequestAsync(result);

            return RedirectToPage("/Ciba/All");
        }

        // we need to redisplay the consent UI
        if (!await SetViewModelAsync(Input.Id))
        {
            return RedirectToPage("/Home/Error/Index");
        }

        return Page();
    }

    private async Task<bool> SetViewModelAsync(string? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var request = await interaction.GetLoginRequestByInternalIdAsync(id);
        if (request is not null && request.Subject.GetSubjectId() == User.GetSubjectId())
        {
            View = CreateConsentViewModel(request);
            return true;
        }

        logger.NoMatchingBackchannelLoginRequest(id);
        return false;
    }

    private ViewModel CreateConsentViewModel(BackchannelUserLoginRequest request)
    {
        var vm = new ViewModel
        {
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            BindingMessage = request.BindingMessage,
            IdentityScopes = request.ValidatedResources.Resources.IdentityResources
                .Select(x => CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name)))
                .ToArray()
        };

        var resourceIndicators = request.RequestedResourceIndicators ?? [];
        var apiResources =
            request.ValidatedResources.Resources.ApiResources.Where(x => resourceIndicators.Contains(x.Name))
                .ToArray();

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
            scopeVm.Resources = apiResources.Where(x => x.Scopes.Contains(parsedScope.ParsedName))
                .Select(x => new ResourceViewModel { Name = x.Name, DisplayName = x.DisplayName ?? x.Name })
                .ToArray();
            apiScopes.Add(scopeVm);
        }

        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
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
            Name = identity.Name,
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
        var displayName = apiScope.DisplayName ?? apiScope.Name;
        if (!string.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

        return new()
        {
            Name = parsedScopeValue.ParsedName,
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
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
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
