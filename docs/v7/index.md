# **Code‑along: Från noll till komplett Identity‑pipeline**

[Nästa: Verifikations ritual ▶](./index-2.md)

---

## **Syfte**

Bygga hela säkerhetskedjan i praktiken:  
**Identity → JWT → Policies → UI‑reaktioner → Verifiering**

***

## **Målet med passet**

*   Gå från *tom databas* till *inloggning med signerad token*.
*   Förstå hur API:t fattar beslut: **AuthN (vem?)** → **AuthZ (får du?)**
*   Skydda POST/PUT/DELETE med **Policies** (Admin/Lärare).
*   Låta GET vara öppet för snabb utveckling.
*   Få Blazor att:
    *   lagra token
    *   skicka token automatiskt
    *   reagera korrekt på **401/403**

***

## **Vad vi bygger steg för steg**

1.  IdentityCore + roller + seeding
2.   som utfärdar en JWT
3.  Middleware‑ordning:  → 
4.  Policies: , 
5.  UI‑stödet: , , RoleGate
6.  ProblemDetails för tydlig felhantering

***

## **Slutmålet**

Kunna följa – och felsöka – hela kedjan:

**UI → API → JWT‑validering → Policy‑beslut → 401/403/2xx → UI‑reaktion**
