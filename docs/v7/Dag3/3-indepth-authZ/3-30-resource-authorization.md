# Resource‑based Authorization

[◀ Föregående: Custom requirements](./3-20-custom-requirements.md) | [Nästa: ProblemDetails & 403‑UX ▶](./3-40-problemdetails-ux.md)

---

## Idé
Ibland vet vi inte om en användare får göra något förrän vi har hämtat objektet från databasen. 
*Exempel: Alla lärare får skapa böcker, men bara läraren som skapade boken får radera den.*

## Flödet
1. Hämta boken: `var book = await _context.Books.FindAsync(id);`
2. Kontrollera: `var result = await _authService.AuthorizeAsync(User, book, "IsBookOwner");`
3. Agera: Om `result.Succeeded` är falskt, returnera `Forbid()`.

![resource-authorization](assets/resource-authorization.svg)

---

[◀ Föregående: Custom requirements](./3-20-custom-requirements.md) | [Nästa: ProblemDetails & 403‑UX ▶](./3-40-problemdetails-ux.md)