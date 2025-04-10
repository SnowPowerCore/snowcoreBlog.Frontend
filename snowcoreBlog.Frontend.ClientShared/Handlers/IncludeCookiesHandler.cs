using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace snowcoreBlog.Frontend.ClientShared.Handlers;

public class IncludeCookiesHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return base.SendAsync(request, cancellationToken);
    }
}