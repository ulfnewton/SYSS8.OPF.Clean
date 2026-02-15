using SYSS8.OPF.Clean.WebApi.Contracts;

using System.Text.Json;

namespace SYSS8.OPF.Clean.WebUi.Services
{
    public class ApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient Client => _httpClientFactory.CreateClient("WebApi");

        public ApiClient(IHttpClientFactory httpClientFactory)
            => _httpClientFactory = httpClientFactory;

        public record LoginResponse(string Token, string Email, string[] Roles);

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var response = await Client.PostAsJsonAsync("/auth/login", new { email, password });
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new ApiProblemException("Inte inloggad eller fel uppgifter")
                {
                    Status = StatusCodes.Status401Unauthorized
                };
            }

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

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return (await response.Content.ReadFromJsonAsync<AuthorDTO>())!;
            }

            Throw(await response.Content.ReadAsStringAsync());
            throw new Exception();
        }

        public async Task<BookDTO> CreateBookAsync(Guid authorId, string title)
        {
            var response = await Client.PostAsJsonAsync(
                "/authors/{authorId}/books",
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
                throw new ApiProblemException($"{pd?.Title}: {pd?.Detail}");
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
