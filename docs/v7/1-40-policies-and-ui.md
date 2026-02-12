# Policies & UI‑beteenden

[◀ Föregående: Auth-pipeline](./ca-1-30-pipeline-concepts.md) | [Nästa: Verifieringsritual ▶](./ca-1-50-verification.md)

## Varför Policies istället för bara Roller?
Vi använder Policies (t.ex. `CanCreateAuthor`) för att frikoppla *vad* man får göra från *vem* man är.
* Idag kräver `CanCreateAuthor` rollen `Admin`.
* Imorgon kanske vi även tillåter rollen `Teacher`.
* Vi ändrar detta på **ett** ställe i `Program.cs`, istället för på varje endpoint.

## UI-ansvaret
Kom ihåg: Att dölja en knapp i Blazor är **UX**, inte säkerhet. En kunnig användare kan fortfarande anropa API:et manuellt. Därför måste säkerheten alltid finnas på servern först.

---
[◀ Föregående: Auth-pipeline](./ca-1-30-pipeline-concepts.md) | [Nästa: Verifieringsritual ▶](./ca-1-50-verification.md)