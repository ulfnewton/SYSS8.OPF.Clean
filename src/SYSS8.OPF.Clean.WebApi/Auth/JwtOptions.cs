namespace SYSS8.OPF.Clean.WebApi.Auth
{
    // Den här klassen håller alla inställningar som behövs för att skapa JWT‑tokens.
    // Vi läser in värdena från konfigurationen (appsettings.json, User Secrets, miljövariabler osv.)
    // i stället för att hårdkoda dem i koden. Det gör systemet säkrare och lättare att ändra.

    public class JwtOptions
    {
        // Issuer = vem som skapar tokenen (t.ex. vårt API).
        public string Issuer { get; set; } = string.Empty;

        // Audience = vem tokenen är avsedd för (t.ex. vår frontend eller våra klienter).
        public string Audience { get; set; } = string.Empty;

        // Key = den hemliga nyckeln som används för att signera tokenen.
        // Viktigt: Den här ska ALDRIG ligga i Git-repot.
        // I utveckling: använd User Secrets.
        // I test/staging/produktion: använd miljövariabler eller Key Vault.
        public string Key { get; set; } = string.Empty;

        // Hur länge en token ska vara giltig (i timmar).
        // Vi styr detta via konfiguration så att vi enkelt kan ändra det utan att röra koden.
        // 8 timmar är ett rimligt standardvärde för demo och utveckling.
        public int ExpiresHours { get; set; } = 8;
    }
}
