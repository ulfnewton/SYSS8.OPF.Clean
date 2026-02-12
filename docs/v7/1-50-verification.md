# Verifieringsritual & Felsökning

[◀ Föregående: Policies & UI](./1-40-policies-and-ui.md) | [Till index ▶](./index.md)

När du är klar med Del 1, kör denna ritual för att säkerställa att allt fungerar:

1.  **Testa 401:** Anropa `POST /authors` utan att skicka med någon token. Du SKA få 401.
2.  **Testa 403:** Logga in som en användare *utan* Admin-roll. Anropa `POST /authors`. Du SKA få 403.
3.  **Testa 201:** Logga in som Admin. Anropa `POST /authors`. Du SKA få 201 Created.

## Vanliga fel (Checklista)
- [ ] **Middleware-ordning:** Ligger `UseAuthentication` före `UseAuthorization`?
- [ ] **Stavfel i Claims:** Heter rollen exakt samma i `JwtTokenService` som i din Policy?
- [ ] **Hänglåset i Swagger:** Har du konfigurerat Swagger så att den faktiskt skickar med din token?

---
[◀ Föregående: Policies & UI](./1-40-policies-and-ui.md) | [Till index ▶](./index.md)