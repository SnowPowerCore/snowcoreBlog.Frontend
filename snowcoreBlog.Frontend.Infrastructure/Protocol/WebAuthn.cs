﻿using Fido2NetLib;
using Microsoft.JSInterop;

namespace snowcoreBlog.Frontend.Infrastructure.Protocol;

/// <summary>
/// Module for accessing the browser's WebAuthn API.
/// </summary>
public class WebAuthn
{
    private IJSObjectReference _jsModule = null!;
    private readonly Task _initializer;

    public WebAuthn(IJSRuntime js)
    {
        _initializer = Task.Run(async () =>
            _jsModule = await js.InvokeAsync<IJSObjectReference>("import", "./js/WebAuthn.js"));
    }

    /// <summary>
    /// Wait for this to make sure this module is initialized.
    /// </summary>
    /// <returns></returns>
    public Task Init() => _initializer;

    /// <summary>
    /// Whether or not this browser supports WebAuthn.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsWebAuthnSupportedAsync() => await _jsModule.InvokeAsync<bool>("isWebAuthnPossible");

    /// <summary>
    /// Creates a new credential.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<AuthenticatorAttestationRawResponse> CreateCredsAsync(CredentialCreateOptions options) =>
        await _jsModule.InvokeAsync<AuthenticatorAttestationRawResponse>("createCreds", options);

    /// <summary>
    /// Verifies a credential for login.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<AuthenticatorAssertionRawResponse> VerifyAsync(AssertionOptions options) =>
        await _jsModule.InvokeAsync<AuthenticatorAssertionRawResponse>("verify", options);
}