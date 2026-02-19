# Verifieringsritual & felsökning

[◀ Föregående: ProblemDetails & 403‑UX](./3-40-problemdetails-ux.md) | [Till index ▶](./3-index.md)

---

1. **Testa Policies:** Fungerar dina namngivna policies i Swagger?
2. **Trigga Handlers:** Sätt en breakpoint i din Handler – körs den när du anropar endpointen?
3. **Resurstest:** Logga in som Användare A och försök redigera Användare B:s data. Får du 403?
4. **Felsökning:** - Har du registrerat din Handler i DI? `builder.Services.AddSingleton<IAuthorizationHandler, MyHandler>();`
   - Stämmer stavningen på policynamn?

---

[◀ Föregående: ProblemDetails & 403‑UX](./3-40-problemdetails-ux.md) | [Till index ▶](./3-index.md)