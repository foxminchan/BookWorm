// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace BookWorm.Identity.Pages;

internal static class Log
{
    private static readonly Action<ILogger, string?, Exception?> _invalidId = LoggerMessage.Define<string?>(
        LogLevel.Error,
        EventIds.InvalidId,
        "Invalid id {Id}");

    private static readonly Action<ILogger, string?, Exception?> _invalidBackchannelLoginId =
        LoggerMessage.Define<string?>(
            LogLevel.Warning,
            EventIds.InvalidBackchannelLoginId,
            "Invalid backchannel login id {Id}");

    private static readonly Action<ILogger, IEnumerable<string>, Exception?> _externalClaims =
        LoggerMessage.Define<IEnumerable<string>>(
            LogLevel.Debug,
            EventIds.ExternalClaims,
            "External claims: {Claims}");

    private static readonly Action<ILogger, string, Exception?> _noMatchingBackchannelLoginRequest =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.NoMatchingBackchannelLoginRequest,
            "No backchannel login request matching id: {Id}");

    private static readonly Action<ILogger, string, Exception?> _noConsentMatchingRequest =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.NoConsentMatchingRequest,
            "No consent request matching request: {ReturnUrl}");

    public static void InvalidId(this ILogger logger, string? id)
    {
        _invalidId(logger, id, null);
    }

    public static void InvalidBackchannelLoginId(this ILogger logger, string? id)
    {
        _invalidBackchannelLoginId(logger, id, null);
    }

    public static void ExternalClaims(this ILogger logger, IEnumerable<string> claims)
    {
        _externalClaims(logger, claims, null);
    }

    public static void NoMatchingBackchannelLoginRequest(this ILogger logger, string id)
    {
        _noMatchingBackchannelLoginRequest(logger, id, null);
    }

    public static void NoConsentMatchingRequest(this ILogger logger, string returnUrl)
    {
        _noConsentMatchingRequest(logger, returnUrl, null);
    }
}

internal static class EventIds
{
    private const int UiEventsStart = 10000;

    //////////////////////////////
    // Consent
    //////////////////////////////
    private const int ConsentEventsStart = UiEventsStart + 1000;
    public const int InvalidId = ConsentEventsStart + 0;
    public const int NoConsentMatchingRequest = ConsentEventsStart + 1;

    //////////////////////////////
    // External Login
    //////////////////////////////
    private const int ExternalLoginEventsStart = UiEventsStart + 2000;
    public const int ExternalClaims = ExternalLoginEventsStart + 0;

    //////////////////////////////
    // CIBA
    //////////////////////////////
    private const int CibaEventsStart = UiEventsStart + 3000;
    public const int InvalidBackchannelLoginId = CibaEventsStart + 0;
    public const int NoMatchingBackchannelLoginRequest = CibaEventsStart + 1;
}
