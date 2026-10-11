using ActualLab.DependencyInjection;
using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Blazor.Authentication;
using ActualLab.Fusion.UI;
using ActualLab.Rpc;
using ActualLab.Rpc.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using snowcoreBlog.PublicApi.Services;

namespace snowcoreBlog.Frontend.AuthorsManagement.Extensions;

/// <summary>
/// Wires up the client side of ActualLab.Fusion for author display name availability.
/// <para>
/// Registers a transparent replica of <see cref="IAuthorDisplayNameAvailabilityService"/>
/// that talks to the AuthorsManagement backend compute service over the Fusion RPC
/// WebSocket, mirroring how <see cref="INickNameAvailabilityService"/> reaches the
/// ReadersManagement backend.
/// </para>
/// <para>
/// <b>Two backends, one container.</b> ActualLab.Rpc keeps a single
/// <see cref="RpcWebSocketClientOptions"/> singleton whose <c>HostUrlResolver</c> and a
/// single <see cref="RpcOutboundCallOptions"/> whose <c>RouterFactory</c> decide where
/// every outbound call goes — a plain second <c>rpc.AddWebSocketClient(url)</c> would be
/// silently ignored (TryAdd). This module therefore replaces both singletons with
/// per-peer routing:
/// <list type="bullet">
/// <item>Compute calls on <see cref="IAuthorDisplayNameAvailabilityService"/> are routed to
/// the peer identified by <see cref="AuthorsPeerHostId"/> and connected to
/// <c>{Fusion:AuthorsGatewayUrl}/fusion/ws</c> (YARP route <c>backend-authorsmanagement-fusion</c>).</item>
/// <item>Every other call (nickname availability, auth, …) keeps routing to
/// <see cref="RpcRef.Default"/> and connects to the ReadersManagement gateway —
/// preserving the behavior ReadersManagement configured earlier (this module
/// MUST be registered AFTER ReadersManagement: its <c>Replace</c> of
/// <see cref="RpcWebSocketClientOptions"/> must be the last registration,
/// since Readers adds its own descriptor via a plain AddSingleton, and the
/// last registration wins in MS.DI).</item>
/// </list>
/// </para>
/// </summary>
public static class FusionConfigurationExtensions
{
    /// <summary>Default gateway base address (matches Apizr client-side configuration).</summary>
    public const string DefaultAuthorsGatewayUrl = "wss://localhost/api/authors";

    /// <summary>Default ReadersManagement gateway base address, kept as the fallback peer URL.</summary>
    public const string DefaultReadersGatewayUrl = "wss://localhost/api/readers";

    /// <summary>
    /// Peer <c>HostInfo</c> identifying the AuthorsManagement Fusion backend.
    /// Used both in outbound call routing and in the per-peer WebSocket URL resolution.
    /// </summary>
    public const string AuthorsPeerHostId = "authors";

    /// <summary>
    /// Relative path appended to the gateway URL; the YARP route strips the
    /// <c>/api/authors</c> prefix so the backend sees its configured <c>/fusion/ws</c>.
    /// </summary>
    public const string FusionWebSocketSuffix = "/fusion/ws";

    public static IServiceCollection AddFusionClient(this IServiceCollection services, IConfiguration configuration)
    {
        var authorsGatewayUrl = NormalizeWebSocketUrl(
            configuration["Fusion:AuthorsGatewayUrl"] ?? DefaultAuthorsGatewayUrl);
        var readersGatewayUrl = NormalizeWebSocketUrl(
            configuration["Fusion:ReadersGatewayUrl"] ?? DefaultReadersGatewayUrl);

        // Idempotent: FusionBuilder reuses the existing FusionTag instance.
        var fusion = services.AddFusion();

        // ComputedStateComponent & friends (CircuitHubComponentBase) inject CircuitHub,
        // which only AddBlazor() registers (scoped) — no-op if Readers already added it.
        fusion.AddBlazor().AddAuthentication().AddPresenceReporter();

        // Transparent replica of the AuthorsManagement compute service.
        fusion.AddClient<IAuthorDisplayNameAvailabilityService>();

        // Per-peer WebSocket URL resolution: the "authors" peer connects to the
        // AuthorsManagement gateway; every other peer (RpcRef.Default and friends)
        // keeps using the ReadersManagement gateway.
        services.Replace(ServiceDescriptor.Singleton(_ => RpcWebSocketClientOptions.Default with {
            HostUrlResolver = peer => peer.Ref.HostInfo switch
            {
                AuthorsPeerHostId => authorsGatewayUrl,
                _ => readersGatewayUrl,
            },
        }));

        // Per-service outbound call routing: author display name availability goes to
        // the "authors" peer, everything else keeps going to RpcRef.Default (readers).
        services.Replace(ServiceDescriptor.Singleton(_ => RpcOutboundCallOptions.Default with {
            RouterFactory = method => _ =>
                method.Service.Type == typeof(IAuthorDisplayNameAvailabilityService)
                    ? RpcRef.NewClient(AuthorsPeerHostId)
                    : RpcRef.Default,
        }));

        // Make sure the WebSocket client exists even if ReadersManagement was never
        // registered (e.g. a module-only host). TryAdd semantics: no-op when present.
        fusion.Rpc.AddWebSocketClient();

        // Default update delay (no-op duplicates are harmless — scoped registration
        // per module would shadow identically, so only add when absent).
        if (!services.HasService<IUpdateDelayer>())
            services.AddScoped<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.25));

        return services;
    }

    /// <summary>
    /// Ensures the gateway URL carries the Fusion WebSocket suffix, converting
    /// scheme-relative URLs (https://) to WebSocket scheme (wss://).
    /// </summary>
    private static string NormalizeWebSocketUrl(string url)
    {
        url = url.Trim().TrimEnd('/');
        if (url.EndsWith(FusionWebSocketSuffix, StringComparison.Ordinal))
            return url;

        if (url.StartsWith("https://", StringComparison.Ordinal))
            url = $"wss://{url["https://".Length..]}";
        else if (url.StartsWith("http://", StringComparison.Ordinal))
            url = $"ws://{url["http://".Length..]}";

        return $"{url}{FusionWebSocketSuffix}";
    }
}
