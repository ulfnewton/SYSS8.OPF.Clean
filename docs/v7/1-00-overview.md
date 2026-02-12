# Översikt & mål – Identity & Jwt

[◀ Till index](./index.md) | [Nästa: AuthN vs AuthZ ▶](./1-10-authn-authz-basics.md)

**Syfte:** Att förstå hur vi säkrar ett API med hjälp av standardiserade JWT-tokens istället för traditionella sessioner.

## Vad du ska kunna efter genomgången
- Förklara varför vi använder **JWT** (stateless) istället för cookies/sessioner (stateful).
- Redogöra för skillnaden mellan **Authentication** (Vem är du?) och **Authorization** (Vad får du göra?).
- Identifiera de tre delarna i en JWT: **Header, Payload** och **Signature**.
- Förstå hur middleware-ordningen i `Program.cs` påverkar säkerheten.
- Utföra en **verifieringsritual** för att bevisa att skyddet fungerar.

---
[◀ Till index](./index.md) | [Nästa: AuthN vs AuthZ ▶](./1-10-authn-authz-basics.md)