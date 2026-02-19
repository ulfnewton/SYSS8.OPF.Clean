# Analogin: Boardingkortet

[◀ Föregående: JWT‑Basics](./1-20-jwt-basics.md) | [Nästa: Auth‑pipeline ▶](./1-30-pipeline-concepts.md)

---

För att förstå varför JWT är så kraftfullt, jämför det med ett boardingkort på en flygplats.

1.  **Incheckning (Login):** Du visar pass (lösenord). Flygbolaget ger dig ett boardingkort (JWT).
2.  **Självcontainrad:** På kortet står ditt namn och om du har "First Class" (Claims). Portvakten vid gaten behöver inte ringa centralen för att kolla om du får gå på – infon står på kortet.
3.  **Signatur (Säkerhetsstämpeln):** Om du försöker ändra "Economy" till "First Class" med en penna kommer stämpeln (signaturen) inte längre att stämma. Vakten ser direkt att biljetten är manipulerad.
4.  **Stateless:** Flygbolaget behöver inte komma ihåg ditt ansikte, de behöver bara lita på att biljetten de själva stämplat är äkta.

---

[◀ Föregående: JWT‑Basics](./1-20-jwt-basics.md) | [Nästa: Auth‑pipeline ▶](./1-30-pipeline-concepts.md)
