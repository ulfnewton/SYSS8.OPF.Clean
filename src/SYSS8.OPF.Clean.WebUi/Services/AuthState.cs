namespace SYSS8.OPF.Clean.WebUi.Services
{
    public class AuthState
    {
        // DESIGN-VAL: Minimalt "auth-minne" för UI (ingen cookie; vi kör Bearer i HttpClient).
        public bool IsAuthenticated { get; private set; }
        public string Role { get; private set; } = string.Empty;
        public string? PreferredName { get; private set; }

        public event Action? OnChanged;


        // RÄTT: Servern talar om "vem" och "vilken roll". UI ska inte gissa.
        public void SignIn(string preferredName, string role)
        {

            if (string.IsNullOrWhiteSpace(preferredName))
            {
                throw new ArgumentException("Name must be non-empty", nameof(preferredName));
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                role = string.Empty; // OBS: vi tillåter roll-lös inloggning i demo
            }

            IsAuthenticated = true;
            Role = role;
            PreferredName = preferredName;
            OnChanged?.Invoke();
        }

        // FIX: Enkel hjälpare för RoleGate/komponenter.
        public bool IsInRole(string role) =>
            !string.IsNullOrWhiteSpace(role) &&
            string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
    }
}
