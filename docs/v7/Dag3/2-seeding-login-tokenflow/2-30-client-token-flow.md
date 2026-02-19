# Klientens tokenflöde – Automatisering

[◀ Inloggning & token](./2-20-login-and-token.md) | [Nästa: Verifieringsritual ▶](./2-40-verification.md)

---

I Blazor vill vi inte manuellt behöva skicka med vår token vid varje enskilt API-anrop. Det är ineffektivt och leder till buggar.

## Lösningen: DelegatingHandler
Tänk på en `DelegatingHandler` som en postsorterare som står mellan din app och internet:
1. Din kod säger: `http.GetAsync("/authors")`.
2. Sorteraren (Handlern) avbryter: *"Vänta, jag kollar i `ITokenStore`... Ah, här är en token!"*.
3. Sorteraren lägger till en header: `Authorization: Bearer [TOKEN]`.
4. Paketet skickas iväg.

## De två reaktionerna i UI
Klienten måste vara förberedd på att servern säger nej:
- **401 (Unauthorized):** "Jag vet inte vem du är (eller din token har gått ut)". -> *Visa logga in-sidan.*
- **403 (Forbidden):** "Jag vet vem du är, men du får inte göra detta". -> *Visa ett felmeddelande: "Behörighet saknas".*

![client-token-flow](../assets/client-token-flow.svg)

---

[◀ Inloggning & token](./2-20-login-and-token.md) | [Nästa: Verifieringsritual ▶](./2-40-verification.md)