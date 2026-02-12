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

        public record LoginResponse(string Token, string Email, string Role);

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var response = await Client.PostAsJsonAsync("/auth/login", new { email, password });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new ApiProblemException("Inte inloggad eller fel uppgifter") { Status = 401 };

            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        }

        public async Task<List<AuthorDTO>> GetAuthorAsync()
        {
            var response = await Client.GetAsync("/authors");
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

        private void Throw(string json)
        {
            try
            {
                var pd = JsonSerializer.Deserialize<ProblemDetailsDto>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var msg = (pd?.Title ?? "Problem") + (pd?.Detail is null ? "" : $": {pd.Detail}");
                throw new ApiProblemException(msg) { Status = pd?.Status ?? 0 };
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