using Microsoft.Extensions.DependencyInjection;
using snowcoreBlog.ApplicationLaunch.Interfaces;
using snowcoreBlog.Frontend.SharedComponents.Features.Antiforgery;
using TimeWarp.State;

namespace snowcoreBlog.Frontend.ClientShared.Services;

public class ClientSharedApplicationLaunchService : IApplicationLaunchService
{
    private readonly IServiceProvider _serviceProvider;

    public ClientSharedApplicationLaunchService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task InitAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        using var antiforgery = scope.ServiceProvider.GetRequiredService<IStore>().GetState<AntiforgeryState>();
        await antiforgery.GetAndSetRequestVerificationToken(CancellationToken.None);
    }
}