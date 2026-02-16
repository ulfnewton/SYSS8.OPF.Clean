namespace SYSS8.OPF.Clean.WebUi.Services
{
    public class AuthState
    {
        // DESIGN-VAL: Minimalt "auth-minne" för UI (ingen cookie; vi kör Bearer i HttpClient).
        public bool IsAuthenticated { get; private set; }
        // INFO: Role används för att spegla serverns behörighet i UI-komponenter.
        public string? Role { get; private set; }
        // INFO: PreferredName visar vem som är inloggad utan att UI behöver tolka claims själv.
        public string? PreferredName { get; private set; }

        // TIPS: UI-komponenter kan lyssna på OnChanged för att uppdatera vyer vid login/logout.
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
            PreferredName = preferredName;
            OnChanged?.Invoke();
        }
    }
}
