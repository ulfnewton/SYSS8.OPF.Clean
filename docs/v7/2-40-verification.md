# Verifieringsritual & felsökning

[◀ Föregående: Klientens tokenflöde](./02-30-client-token-flow.md) | [Till index ▶](./02-index.md)

---

Innan vi går vidare till nästa stora modul måste vi veta att fundamentet håller. Kör denna ritual systematiskt.

## Teststeg
1. **Login:** Logga in i Swagger eller UI. Kopiera din token och klistra in den i [jwt.io](https://jwt.io). *Ser du dina roller i listan?*
2. **401-Test:** Rensa din token (logga ut/töm storage) och försök skapa en författare. Får du 401?
3. **403-Test:** Logga in som en vanlig "Lärare". Försök göra en Admin-åtgärd (t.ex. ta bort data). Får du 403?
4. **2xx-Test:** Logga in som "Admin". Gör samma åtgärd. Går det igenom?

## Diagnos – Om det inte fungerar:
- **Alltid 401?** Kolla så att `Issuer` och `Audience` i `appsettings.json` är identiska i både WebApi och WebUi.
- **Alltid 403?** Kolla om din Policy i `Program.cs` kräver rollen `"Admin"` men din seeding skapar rollen `"admin"` (skiftlägeskänsligt!).
- **Token syns inte?** Kolla i webbläsarens *Network*-tab. Finns `Authorization`-headern med i anropet?

---

[◀ Föregående: Klientens tokenflöde](./02-30-client-token-flow.md) | [Till index ▶](./02-index.md)