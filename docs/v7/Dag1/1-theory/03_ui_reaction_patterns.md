# UI-reaktioner på auth-fel

[◀ Föregående](./02_statuscodes_problemdetails.md) | [Nästa ▶](../2-workshop/1-policy-matrix.md)

---

## Mönster
- **Visa/Dölj**: Rendera inte skyddade knappar/länkar för saknade behörigheter.
- **Disable**: Visa kontroller men disable vid saknad behörighet.
- **Felpanel**: Visa ProblemDetails vid 401/403.

```mermaid
flowchart LR
  AuthState[Auth State] --> Guard{Har Policy?}
  Guard -->|Ja| Show[Visa kontroll]
  Guard -->|Nej| Hide[Dölj/Disable]
  Error[HTTP svar] -->|401/403 med ProblemDetails| Panel[Felpanel]
```

---

[◀ Föregående](./02_statuscodes_problemdetails.md) | [Nästa ▶](../2-workshop/1-policy-matrix.md)
