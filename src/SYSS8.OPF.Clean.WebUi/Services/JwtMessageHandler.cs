using System.Net.Http.Headers;

namespace SYSS8.OPF.Clean.WebUi.Services;

public sealed class JwtMessageHandler : DelegatingHandler
{
    private readonly ITokenStore _tokens;

    public JwtMessageHandler(ITokenStore tokens)
        => _tokens = tokens;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokens.AccessToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}