
# Uppgift: C4 – System → Containers → Components

**Syfte.** Att förstå systemet som helhet genom att berätta om det på tre nivåer – från omvärldsrelationer till komponentansvar. Övningen bygger på C4‑avsnitten i *Systemtänkande_v9d2.pptx*. citeturn2search1

**Vad du gör.**
Du skapar tre enkla, tydliga bilder:

## 1) System Context
Berätta vilka som möter systemet och hur:
- Externa användare
- Blazor‑UI
- Minimal API
- SQLite
Beskriv huvudflöden och viktiga rättighetsaspekter (exempelvis skillnaden mellan 401 och 403) i korta etiketter.

## 2) Containers
Visa de tre huvuddelarna och hur de samspelar:
- UI (Blazor) – presentation och händelser
- API (Minimal API) – kontrakt och logiknav
- Databas (SQLite) – persistens
Nämn kommunikationen mellan dem (t.ex. HTTP och EF Core) där det hjälper förståelsen.

## 3) Components
Zooma in i API‑delen och fördela ansvar i en tydlig kedja:
**UI → Endpoint → Handler → Repository → Databas**
Markera var domänregler hör hemma (Domain), var applikationslogik körs (Application) och hur fel uttrycks enhetligt (ProblemDetails).

**Resultat.** Tre bilder (L1, L2, L3) som tillsammans gör det lätt för en utomstående att förstå ert system utan att läsa koden.
