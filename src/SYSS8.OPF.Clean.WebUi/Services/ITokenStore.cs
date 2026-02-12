namespace SYSS8.OPF.Clean.WebUi.Services;

public interface ITokenStore
{
    string? AccessToken { get; }
    void SetToken(string? token);
    event Action? Changed;
}