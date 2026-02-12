# JWT – Struktur, Claims och Signatur

[◀ Föregående: AuthN vs AuthZ](./ca-1-10-authn-authz-basics.md) | [Nästa: Boardingkortet ▶](./ca-1-25-the-boarding-pass.md)

En JWT (JSON Web Token) är en självcontainrad bärare av information. Det betyder att servern inte behöver fråga databasen vid varje anrop – all info finns i "biljetten".

## Tokens tre delar
1.  **Header (Röd):** Säger vilken algoritm som används (oftast HS256).
2.  **Payload (Lila):** Innehåller **Claims**. Detta är din data (ID, Email, Roller, Utgångsdatum).
3.  **Signature (Blå):** En matematisk hash av headern + payloaden + en hemlig nyckel på servern.



> **VARNING:** Payload är bara Base64-kodad, *inte* krypterad. Vem som helst kan läsa innehållet på t.ex. [jwt.io](https://jwt.io). Spara aldrig lösenord eller känslig info i en JWT!

---
[◀ Föregående: AuthN vs AuthZ](./ca-1-10-authn-authz-basics.md) | [Nästa: Boardingkortet ▶](./ca-1-25-the-boarding-pass.md)