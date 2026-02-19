# ProblemDetails & 403‑UX

[◀ Föregående: Resource‑based](./3-30-resource-authorization.md) | [Nästa: Verifieringsritual ▶](./3-50-verification.md)

---

## Varför ProblemDetails?
Det är ett standardiserat sätt (RFC 7807) för ett API att skicka felmeddelanden. Istället för bara "403", skickar vi ett JSON-objekt som förklarar felet.

## Klientens reaktion
- **401 (Unauthorized):** "Jag vet inte vem du är". UI bör skicka användaren till Login.
- **403 (Forbidden):** "Jag vet vem du är, men du får inte göra detta". UI bör visa ett felmeddelande i en röd panel eller dölja knappen helt.

![error-contract](../assets/error-contract.svg)

---

[◀ Föregående: Resource‑based](./3-30-resource-authorization.md) | [Nästa: Verifieringsritual ▶](./3-50-verification.md)