using System.Net.Http.Headers;

namespace SYSS8.OPF.Clean.WebUi.Services
{

    // DESIGN-VAL: DelegatingHandler som automatiskt sätter Authorization: Bearer <token>.
    // Håller "bearer-logiken" på ett ställe → API-anrop i koden blir renare.
    public class JwtMessageHandler : DelegatingHandler
    {
        private readonly ITokenStore _tokens;

        public JwtMessageHandler(ITokenStore tokens)
        {
            _tokens = tokens;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct = default)
        {
            var token = _tokens.AccessToken;

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, ct);
        }
    }
}
