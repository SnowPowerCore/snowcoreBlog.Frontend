using ActualLab.Fusion;
using ActualLab.Fusion.Authentication;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Blazor.Authentication;
using ActualLab.Fusion.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.PublicApi.Services;

namespace snowcoreBlog.Frontend.ReadersManagement.Extensions;

/// <summary>
/// Wires up the client side of ActualLab.Fusion for nickname availability.
/// <para>
/// Registers a transparent replica of <see cref="INickNameAvailabilityService"/> that talks
/// to the backend compute service over the Fusion RPC WebSocket. The replica shares the same
/// interface, so callers simply <c>await</c> <see cref="INickNameAvailabilityService.GetAsync"/>
/// and Fusion keeps the result coherent and auto-refreshed on invalidation.
/// </para>
/// <para>
/// The backend WebSocket endpoint lives behind the YARP gateway at
/// <c>{gateway}/api/readers/fusion/ws</c> (route <c>backend-readersmanagement-fusion</c>),
/// mirroring the existing Apizr base address <c>https://localhost/api/readers</c>.
/// </para>
/// </summary>
public static class FusionConfigurationExtensions
{
    /// <summary>Default gateway base address (matches Apizr client-side configuration).</summary>
    public const string DefaultReadersGatewayUrl = "wss://localhost/api/readers";

    /// <summary>
    /// Relative path appended to the gateway URL; the YARP route strips the
    /// <c>/api/readers</c> prefix so the backend sees its configured <c>/fusion/ws</c>.
    /// </summary>
    public const string FusionWebSocketSuffix = "/fusion/ws";

    public static IServiceCollection AddFusionClient(this IServiceCollection services, IConfiguration configuration)
    {
        var gatewayUrl = configuration["Fusion:ReadersGatewayUrl"]
            ?? DefaultReadersGatewayUrl;

        var fusion = services.AddFusion();

        fusion.AddAuthClient();
        // ComputedStateComponent & friends (CircuitHubComponentBase) inject CircuitHub,
        // which only AddBlazor() registers (scoped). Required by NickNameAvailabilityIndicator.
        fusion.AddBlazor().AddAuthentication().AddPresenceReporter();

        fusion.AddClient<INickNameAvailabilityService>();

        fusion.Rpc.AddWebSocketClient($"{gatewayUrl}{FusionWebSocketSuffix}");
        
        // Default update delay
        services.AddScoped<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.25));

        return services;
    }

    /// <summary>
    /// Resolves the absolute Fusion WebSocket URL for the current Blazor host.
    /// Used by components that need to display or debug the connection target.
    /// Works for both WASM (browser origin) and Blazor Server (gateway origin).
    /// </summary>
    public static string ResolveFusionWebSocketUrl(NavigationManager navigation, IConfiguration configuration)
    {
        var gatewayUrl = configuration["Fusion:ReadersGatewayUrl"] ?? DefaultReadersGatewayUrl;
        var baseUri = navigation.BaseUri;
        // Prefer the configured absolute gateway URL; fall back to the app's origin.
        var root = Uri.IsWellFormedUriString(gatewayUrl, UriKind.Absolute)
            ? gatewayUrl
            : new Uri(new Uri(baseUri), gatewayUrl).ToString();
        return $"{root.TrimEnd('/')}{FusionWebSocketSuffix}";
    }
}
