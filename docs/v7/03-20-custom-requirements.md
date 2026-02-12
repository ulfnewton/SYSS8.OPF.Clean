# Custom Requirements & Handlers

[◀ Föregående: Policy‑design](./03-10-policy-design.md) | [Nästa: Resource‑based ▶](./03-30-resource-authorization.md)

---

När standardregler inte räcker bygger vi egna logiska vakter.

## Byggstenarna
1. **Requirement:** En klass som implementerar `IAuthorizationRequirement`. Den fungerar som en kravbeskrivning (t.ex. "Måste vara ägare").
2. **Handler:** En klass som implementerar `AuthorizationHandler<T>`. Här bor den faktiska C#-koden som kollar claims, databaser eller klockan.



## Hantering av beslut
I din Handler anropar du `context.Succeed(requirement)` för att släppa igenom användaren. Om du inte gör något, eller anropar `context.Fail()`, nekas åtkomst.

![requirement-handler](assets/requirement-handler.svg)

---

[◀ Föregående: Policy‑design](./03-10-policy-design.md) | [Nästa: Resource‑based ▶](./03-30-resource-authorization.md)