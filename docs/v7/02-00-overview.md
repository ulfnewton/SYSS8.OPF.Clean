# Översikt & mål – Från Databas till Token

[◀ Till index](./02-index.md) | [Nästa: Seeding ▶](./02-10-identity-seeding.md)

**Syfte:** Att gå från en tom databas till ett system där vi kan identifiera oss och bära med oss vår identitet genom hela applikationen.

## Vad du får koll på
- **Automatiserad Seeding:** Hur vi garanterar att roller (Admin, Lärare) och testkonton alltid finns på plats vid start.
- **Inloggningsmekaniken:** Hur `/login` omvandlar användaruppgifter till en signerad JWT.
- **Klientens minne:** Hur Blazor-appen sparar din token och automatiskt bifogar den i varje "paket" (HTTP-anrop) som skickas till API:et.

## Det stora sammanhanget
I förra delen satte vi upp vakten (Middleware). I denna del skapar vi **passerkorten** (Tokens) och ser till att de boende (Användarna) har rätt behörigheter inskrivna i sina kort.

---
[◀ Till index](./02-index.md) | [Nästa: Seeding ▶](./02-10-identity-seeding.md)