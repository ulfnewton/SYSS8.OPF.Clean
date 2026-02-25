
# Plan för dagen – Systemtänkande v9d2

Denna plan leder dig genom dagen i en sammanhängande berättelse: **först förstå helheten (C4), sedan motivera beslut (ADR), och till sist pröva ansvar och gränser (Microservices)**. Den följer samma struktur som presentationen *Systemtänkande_v9d2.pptx*. citeturn2search1

## Förmiddag: C4 – se helheten
Du börjar med att skapa tre bilder som tillsammans berättar om ert systems helhet:

**System Context** beskriver vilka som använder systemet och hur de möter det. Här syns externa användare, **Blazor‑UI**, **Minimal API** och **SQLite** samt de viktigaste interaktionerna. 

**Containers** visar de tre stora delarna: användargränssnittet (UI), API:t som kontrakt och logiknav, samt databasen. Du beskriver hur de samspelar via HTTP och EF Core.

**Components** zoomar in på API‑delen: en tydlig kedja **UI → Endpoint → Handler → Repository → Databas** där ansvar, validering och felmodell (**ProblemDetails**) blir synliga.

## Mitt på dagen: ADR – motivera ett arkitekturbeslut
När helheten är synlig dokumenterar du ett verkligt beslut i projektet. Du skriver en ADR som svarar på fyra frågor: **Context**, **Decision**, **Alternatives**, **Consequences**. Välj ett beslut som påverkar hela systemet – t.ex. statuskodspolicy, ProblemDetails, Minimal API framför MVC eller varför EF Core ligger i Infrastructure.

## Eftermiddag: Microservices‑tänk – gränser och kontrakt
Nu prövar du tankesättet bakom microservices utan att bygga dem: markera två möjliga gränser (t.ex. Thread och Comment), formulera deras kontrakt (2–3 endpoints vardera) och resonerar kort om varför er nuvarande **modulära monolit** är lämplig idag samt när en uppdelning skulle vara motiverad.

## Avslutning: Exit reflection
Du knyter ihop dagen med en kort reflektion: vad blev tydligast, vad tar du vidare i projektet och hur har din bild av helheten förändrats?
