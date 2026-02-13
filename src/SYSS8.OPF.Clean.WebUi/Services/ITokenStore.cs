namespace SYSS8.OPF.Clean.WebUi.Services
{
    public interface ITokenStore
    {
        string? AccessToken { get; }
        void SetToken(string? token);
        event Action? OnChanged;
    }
    public sealed class TokenStore : ITokenStore
    {
        private string? _token;

        public string? AccessToken => _token;

        public event Action? OnChanged;

        public void SetToken(string? token)
        {
            _token = token;
            OnChanged?.Invoke();
        }
    }
}
