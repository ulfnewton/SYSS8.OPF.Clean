namespace SYSS8.OPF.Clean.WebUi.Services
{
    public class AuthState
    {
        // DESIGN-VAL: Minimalt "auth-minne" för UI (ingen cookie; vi kör Bearer i HttpClient).
        public bool IsAuthenticated { get; private set; }
        public string[] Roles { get; private set; } = Array.Empty<string>();
        public string? PreferredName { get; private set; }

        public event Action? OnChanged;


        // RÄTT: Servern talar om "vem" och "vilka roller". UI ska inte gissa.
        public void SignIn(string preferredName, string[] roles)
        {

            if (string.IsNullOrWhiteSpace(preferredName))
            {
                throw new ArgumentException("Name must be non-empty", nameof(preferredName));
            }

            if (roles is null || roles.Length == 0)
            {
                roles = Array.Empty<string>(); // OBS: vi tillåter roll-lös inloggning i demo
            }

            IsAuthenticated = true;
            Roles = roles;
            PreferredName = preferredName;
            OnChanged?.Invoke();
        }

        // FIX: Enkel hjälpare för RoleGate/komponenter.
        public bool IsInRole(string role) =>
            !string.IsNullOrWhiteSpace(role) &&
            Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }
}
