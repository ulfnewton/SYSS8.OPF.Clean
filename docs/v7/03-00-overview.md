# 001 Översikt & mål – Auktorisering på djupet

[◀ Till index](./03-index.md) | [Nästa: Policy‑design ▶](./03-10-policy-design.md)

---

**Syfte:** Att gå från enkla "är du admin?"-frågor till smart logik som kan svara på: *"Får just den här användaren ändra just den här resursen just nu?"*.

## Vad du får koll på
- **Avancerad Policy-design:** Hur man kombinerar krav med AND/OR-logik.
- **Custom Requirements:** Hur vi skapar egna regler (t.ex. "Måste vara över 18 år" eller "Måste tillhöra rätt avdelning").
- **Resursbaserad Auktorisering:** Att låsa specifika rader i databasen baserat på ägarskap.
- **Tydlig felhantering:** Hur vi använder `ProblemDetails` för att berätta för klienten *varför* det tog stopp.

## Varför detta steg?
Verkliga applikationer är sällan svartvita. Genom att bemästra Handlers och Requirements kan du bygga komplexa affärsregler som är lätta att underhålla och testa utan att kladda ner dina controllers med if-satser.

---

[◀ Till index](./03-index.md) | [Nästa: Policy‑design ▶](./03-10-policy-design.md)