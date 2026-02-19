# Inloggning & tokenutfärdande

[◀ Seeding](./2-10-identity-seeding.md) | [Nästa: Klientens tokenflöde ▶](./2-30-client-token-flow.md)

---

## Vad händer vid en inloggning?
Inloggningen är den enda gången vi faktiskt använder lösenordet. Därefter glömmer vi det och använder bara vår JWT.

1. **Validering:** `UserManager` kollar om e-post och lösenord matchar.
2. **Claim-insamling:** Vi hämtar användarens roller (t.ex. Admin) och lägger till dem som "Claims" (påståenden) i token.
3. **Signering:** Servern använder sin `SecretKey` för att signera token. Detta är "sigillet" som gör att ingen kan förfalska sina roller senare.

## Anatomins Claims
När du packar upp din token (t.ex. på [jwt.io](https://jwt.io)) ser du:
- `sub`: Användarens unika ID (Guid).
- `role`: "Admin" (Detta är vad din Policy letar efter!).
- `exp`: När biljetten slutar gälla.

![login-seq](../assets/login-sequence.svg)

---

[◀ Seeding](./2-10-identity-seeding.md) | [Nästa: Klientens tokenflöde ▶](./2-30-client-token-flow.md)