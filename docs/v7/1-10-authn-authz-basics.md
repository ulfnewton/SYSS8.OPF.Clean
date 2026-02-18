# AuthN vs AuthZ – 401 kontra 403

[◀ Föregående: Översikt](./1-00-overview.md) | [Nästa: JWT-Basics ▶](./1-20-jwt-basics.md)

För att felsöka säkerhet måste man veta vilken fråga API:et misslyckas med.

## 1. Authentication (AuthN) – "Vem är du?"
* **Mål:** Bekräfta användarens identitet.
* **Verktyg:** Inloggning -> JWT skapas.
* **Misslyckande:** Om token saknas eller är trasig svarar servern med **401 Unauthorized**.
* *Tänk:* "Du kommer inte ens in i byggnaden."

## 2. Authorization (AuthZ) – "Vad får du göra?"
* **Mål:** Kontrollera rättigheter (roller/policies).
* **Verktyg:** Claims i din token (t.ex. `role: Admin`).
* **Misslyckande:** Om du är inloggad men försöker göra något du inte får (t.ex. en student som försöker ta bort en lärare) svarar servern med **403 Forbidden**.
* *Tänk:* "Du är inne i byggnaden, men dörren till arkivet är låst."

---
[◀ Föregående: Översikt](./1-00-overview.md) | [Nästa: JWT-Basics ▶](./1-20-jwt-basics.md)