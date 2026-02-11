namespace SYSS8.OPF.Clean.WebUi.Services
{
    public class AuthState
    {
        public bool IsAuthenticated { get; private set; }
        public string? Role { get; private set; }
        public string? PreferredNamed {  get; private set; }

        public event Action? OnChanged;

        public void SignIn(string role, string preferredName)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException(
                    "Role must be non-empty",
                    nameof(role));
            }
            if (string.IsNullOrEmpty(preferredName))
            {
                throw new ArgumentException(
                    "Name must be non-empty",
                    nameof(preferredName));
            }

            IsAuthenticated = true;
            Role = role;
            PreferredNamed = preferredName;
            OnChanged?.Invoke();
        }
    }
}
