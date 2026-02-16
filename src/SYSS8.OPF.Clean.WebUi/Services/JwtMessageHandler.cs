using System.Net.Http.Headers;

namespace SYSS8.OPF.Clean.WebUi.Services;

public sealed class JwtMessageHandler : DelegatingHandler
{
    private readonly ITokenStore _tokens;

    // DESIGN-VAL: DelegatingHandler centraliserar Bearer-headern så varje API-anrop förblir rent.
    public JwtMessageHandler(ITokenStore tokens)
        => _tokens = tokens;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // INFO: Separation of concerns – handlern ansvarar för auth, klienten för affärsanrop.
        var token = _tokens.AccessToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}