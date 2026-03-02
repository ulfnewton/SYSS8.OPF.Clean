using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using SYSS8.OPF.Clean.WebApi.Contracts;

namespace SYSS8.OPF.Clean.WebUi.Services
{
    public class ApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient Client => _httpClientFactory.CreateClient("WebApi");

        public ApiClient(IHttpClientFactory httpClientFactory)
            => _httpClientFactory = httpClientFactory;

        public record LoginResponse(string Token, string Email, string Role);

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var response = await Client.PostAsJsonAsync("/auth/login", new { email, password });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await ThrowAsync(response);

            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        }

        public async Task<List<AuthorDTO>> GetAuthorAsync()
        {
            var response = await Client.GetAsync("/authors");

            if (!response.IsSuccessStatusCode)
                await ThrowAsync(response);

            return await response.Content.ReadFromJsonAsync<List<AuthorDTO>>() ?? [];
        }

        public async Task<AuthorDTO> CreateAuthorAsync(string name)
        {
            var response = await Client.PostAsJsonAsync("/authors", new AuthorDTO(name));

            if (response.StatusCode == HttpStatusCode.Created)
                return (await response.Content.ReadFromJsonAsync<AuthorDTO>())!;

            await ThrowAsync(response);
            throw new InvalidOperationException("Unreachable");
        }

        public async Task<BookDTO> CreateBookAsync(Guid authorId, string title)
        {
            var response = await Client.PostAsJsonAsync($"/authors/{authorId}/books", new BookDTO(title));

            if (response.StatusCode == HttpStatusCode.Created)
                return (await response.Content.ReadFromJsonAsync<BookDTO>())!;

            await ThrowAsync(response);
            throw new InvalidOperationException("Unreachable");
        }

        public async Task DeleteAuthorAsync(Guid authorId)
        {
            var response = await Client.DeleteAsync($"/authors/{authorId}");

            if (response.IsSuccessStatusCode) return;

            await ThrowAsync(response);
        }

        private static readonly JsonSerializerOptions ProblemJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private async Task ThrowAsync(HttpResponseMessage response)
        {
            ProblemDetailsDto? pd = null;

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            var looksLikeJson =
                string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase);

            if (looksLikeJson)
            {
                try
                {
                    pd = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(ProblemJsonOptions);
                }
                catch
                {
                }
            }

            if (pd is not null && (!string.IsNullOrWhiteSpace(pd.Title) || !string.IsNullOrWhiteSpace(pd.Detail)))
            {
                throw new ApiProblemException(
                    message: $"{pd.Title}: {pd.Detail}",
                    problem: pd,
                    status: pd.Status ?? (int)response.StatusCode
                );
            }

            var text = await response.Content.ReadAsStringAsync();
            var safe = string.IsNullOrWhiteSpace(text)
                ? response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}"
                : text.Length > 400 ? text.AsSpan(0, 400).ToString() + "…" : text;

            throw new ApiProblemException(safe)
            {
                Status = (int)response.StatusCode
            };
        }
    }

    public class ProblemDetailsDto
    {
        public string? Title { get; set; }  // what went wrong (short)
        public string? Detail { get; set; } // why it went wrong (actionable)
        public int? Status { get; set; }    // HTTP code (e.g., 401/403/404/409)
        public string? Type { get; set; }   // optional link to a spec or docs
    }

    public class ApiProblemException : Exception
    {
        public int Status { get; set; }                    // numerisk HTTP-statuskod (e.g., 401/403/404/409)
        public ProblemDetailsDto? Problem { get; }         // varlfri ProblemDetailsDto med mer info (Title, Detail, Type)

        public ApiProblemException(string? message) : base(message) { }

        public ApiProblemException(string? message, ProblemDetailsDto? problem, int status)
            : base(message)
        {
            Problem = problem;
            Status = status;
        }
    }
}
