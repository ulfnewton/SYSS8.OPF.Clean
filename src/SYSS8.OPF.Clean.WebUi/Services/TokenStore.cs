namespace SYSS8.OPF.Clean.WebUi.Services;

public sealed class TokenStore : ITokenStore
{
    private string? _token;
    public string? AccessToken => _token;

    public event Action? Changed;

    public void SetToken(string? token)
    {
        _token = token;
        Changed?.Invoke();
    }
}