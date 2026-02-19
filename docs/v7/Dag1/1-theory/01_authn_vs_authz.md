
# AuthN vs AuthZ – översikt

**AuthN (Authentication)** svarar på *vem* användaren är. **AuthZ (Authorization)** svarar på *vad* användaren får göra.

## Kedjan UI → API → App → Domain → Infra
```mermaid
flowchart LR
  UI[UI] --> API[Web API]
  API --> APP[Application]
  APP --> DOMAIN[Domain]
  DOMAIN --> INFRA[Infrastructure]

  classDef s fill:#1976D2,color:#fff,stroke:#0D47A1,stroke-width:2px
  classDef p fill:#388E3C,color:#fff,stroke:#1B5E20,stroke-width:2px
  class UI,API,APP,DOMAIN,INFRA s
```

## Beslutsplacering
- **AuthN**: sker vid **API**-gränsen (middleware/handler).
- **AuthZ**: uttrycks som **policies** på endpoints/kontrollers.

## 401 vs 403
```mermaid
flowchart TD
  A[Har klienten autentiserat sig?] -->|Nej| U401[401 Unauthorized]
  A -->|Ja| B[Uppfyller klienten policyn?]
  B -->|Nej| U403[403 Forbidden]
  B -->|Ja| OK[Tillåten]

  classDef good fill:#388E3C,color:#fff
  classDef warn fill:#F57C00,color:#fff
  classDef bad fill:#C62828,color:#fff
  class OK good
  class U401,U403 bad
```
