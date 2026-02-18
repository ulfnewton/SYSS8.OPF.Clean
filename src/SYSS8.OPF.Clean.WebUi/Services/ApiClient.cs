using System.Net;
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

        // INFO: Role skickas från servern för att UI ska spegla verklig behörighet.
        public record LoginResponse(string Token, string Email, string Role);

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var response = await Client.PostAsJsonAsync("/auth/login", new { email, password });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new ApiProblemException("Inte inloggad eller fel uppgifter") { Status = 401 };

            // TIPS: EnsureSuccessStatusCode ger en tydlig brytpunkt för oväntade fel.
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        }

        public async Task<List<AuthorDTO>> GetAuthorAsync()
        {
            var response = await Client.GetAsync("/authors");
            // INFO: Vi låter HttpClient kasta om status inte är 2xx för att hålla flödet rakt.
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AuthorDTO>>() ?? [];
        }

        public async Task<AuthorDTO> CreateAuthorAsync(string name)
        {
            var response = await Client.PostAsJsonAsync(
                "/authors",
                new AuthorDTO(name));

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new ApiProblemException("401 Unauthorized") { Status = 401 };
            if (response.StatusCode == HttpStatusCode.Forbidden)
                throw new ApiProblemException("403 Forbidden") { Status = 403 };

            if (response.StatusCode == HttpStatusCode.Created)
                return (await response.Content.ReadFromJsonAsync<AuthorDTO>())!;

            Throw(await response.Content.ReadAsStringAsync());
            throw new Exception();
        }

        public async Task<BookDTO> CreateBookAsync(Guid authorId, string title)
        {
            var response = await Client.PostAsJsonAsync(
                $"/authors/{authorId}/books",      // FIX: interpolera
                new BookDTO(title));

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return (await response.Content.ReadFromJsonAsync<BookDTO>())!;
            }

            Throw(await response.Content.ReadAsStringAsync());
            return null!;
        }

        public async Task DeleteAuthorAsync(Guid authorId)
        {
            var response = await Client.DeleteAsync($"/authors/{authorId}");

            if (response.IsSuccessStatusCode) return;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new ApiProblemException("Inte inloggad eller fel uppgifter")
                {
                    Status = StatusCodes.Status401Unauthorized
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ApiProblemException("Saknar behörighet att ta bort författare")
                {
                    Status = StatusCodes.Status403Forbidden
                };
            }

            Throw(await response.Content.ReadAsStringAsync());
        }

        private void Throw(string json)
        {
            try
            {
                var pd = JsonSerializer.Deserialize<ProblemDetailsDto>(
                    json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });
                throw new ApiProblemException($"{pd?.Title}: {pd?.Detail}")
                {
                    Status = pd?.Status ?? 0
                };
            }
            catch
            {
                throw new ApiProblemException(json);
            }
        }
    }

    public class ProblemDetailsDto
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
    }

    public class ApiProblemException : Exception
    {
        public int Status { get; set; }
        public ApiProblemException(string? message) : base(message) { }
    }
}
