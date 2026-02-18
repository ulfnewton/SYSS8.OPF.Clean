med# Workshop - Given/When/Then - 2026-02-18

## Målet med passet

*   Ni arbetar **i par** med en Driver (skriver) och en Navigator (kommer med förslag, ställer frågor, osv.). Byte efter halva tiden eller när ni har gjort tre scenarier. 
*   Poängen är **inte** att “testa GWT‑språket” i sig, utan att **formulera alla delar tydligt före körning**:

    - *Given* (identitet/roll + data + målmiljö)
    - *When* (exakt anrop)
    - *Then* (statuskod/response/ev. databasändring)

    **och att peka ut var beslutet tas** (JWT, policy, validering, affärsregel/DB, miljö).

*   Ni väljer **valfria endpoints/resurser**, så länge ni skriver **minst sex GWT** som var och en uppfyller en av dessa typer: **lyckad (201/200/204)**, **401**, **403**, **400**, **404**, **409**.

> **Vi samlas för gemensam genomgång kl. 15:30–16:00.**

***

## Steg A — Skriv era sex GWT‑scenarier (valfria endpoints)

**Mall (kopiera till anteckningar):**

    Scenario: <kort rubrik>
    Given:   <identitet/roll + nödvändig data + målmiljö "Test">
    When:    <metod + endpoint + headers + payload> (exakt och upprepningsbart)
    Then:    <förväntad statuskod> [+ ProblemDetails/Location vid behov]
    Var tas beslutet? <JWT / Policy / Validering / Affärsregel / DB / Miljö>

**Krav på täckning (minst ett scenario per typ):**

*   **Lyckat** → 201 (POST + Location) eller 200/204 där det passar.
*   **401** → identitet saknas/ogiltig (authN).
*   **403** → identitet finns men saknar rättighet (authZ/policy).
*   **400** → validering (fel eller saknade fält).
*   **404** → resurs saknas.
*   **409** → konflikt/dubblett (affärsregel/unikhet).

**Tips när ni skriver:**

*   Given ska namnge **roll/claims**, **nödvändig data** och **målmiljö** (t.ex. “Test”).
*   When måste gå att köra **1:1** (metod, endpoint, headers, payload).
*   Then ska vara **verifierbar** (statuskod + ev. Location/ProblemDetails).
*   Svara alltid på frågan **“Var tas beslutet?”** – det styr er felsökning imorgon.

***

## Steg B — Bygg er `.http` (generellt, utan att låsa endpoints)

> **Gäller i denna kurs:**  
> • **Endast a–z och 0–9** i alla variabel‑ och requestnamn (inga bindestreck/understreck/mellanslag).  
> • Vi använder **namngivna login‑requests** och läser ut `token` ur svaret.  
> • Vi skapar **header‑variabler** som innehåller **hela** `Authorization`‑raden och lägger in dem som **egen rad** i varje scenario‑request.

### 1) Basvariabel (överst)

```http
@baseurl = https://<er-test-eller-staging-host>
```

När vi kör tester i Development är hosten `https://localhost:5001`.

### 2) Login‑requests (körs först – ni anpassar endpoint/payload efter ert API)

> Namn: endast a–z/0–9.
> Byt ut [HTTP-METOD] mot rätt Http-metod.

```http
# @name loginteacher
[HTTP-METOD] {{baseurl}}/<auth-login-endpoint>
Content-Type: application/json

{
  "email": "teacher@example.com",
  "password": "Password1!"
}
```

```http
# @name loginstudent
[HTTP-METOD] {{baseurl}}/<auth-login-endpoint>
Content-Type: application/json

{
  "email": "student@example.com",
  "password": "Password1!"
}
```

### 3) Token‑header‑variabler (exakt så här ska det stå)

> Dessa variabler innehåller **hela** header‑raden och ska **infogas som egen rad** i requests som kräver token.

```http
@tokenlarare = Authorization: Bearer {{loginstudent.response.body.$.token}}
@tokenstudent = Authorization: Bearer {{loginteacher.response.body.$.token}}
```

> **Obs:** Detta är den form vi använder i kursen. Lägg inte `Bearer …` i samma rad som någon annan header.

### 4) Scenario‑request (generisk mall – anpassa till ert GWT)

> Infoga `{{tokenlarare}}` eller `{{tokenstudent}}` som **egen header‑rad** när scenariot kräver token.

```http
# scenario <egen-beskrivning> <förväntad-kod>
# given <roll/claims + data + målmiljö>
# when  <metod och endpoint ni valt>
# then  <förväntad statuskod> [+ Location/ProblemDetails]

<HTTP-METOD> {{baseurl}}/<valfri-endpoint>
{{tokenlarare}}
Content-Type: application/json

{ /* er payload enligt kontraktet */ }
```

**Exempelrader att återanvända (beroende på ert Then):**

*   **Lyckat POST → 201**: kontrollera att Location finns i svaret.
*   **400** (validering): skicka payload som triggar `errors.<fält>` i ProblemDetails.
*   **404**: anropa med id/nyckel som inte finns.
*   **409**: skapa samma resurs två gånger (eller vad som i ert API ger konflikt).
*   **401**: utelämna token‑raden helt.
*   **403**: använd token‑headern som representerar “fel” roll för just er endpoint.

***

## Egenkontroll innan 15:30

*   Alla sex scenarier finns i **GWT‑form** med tydlig Given/When/Then och “**Var tas beslutet?**”.
*   `.http` innehåller: **@baseurl**, **loginteacher**, **loginstudent**, **@tokenlarare**, **@tokenstudent**, samt **en request per scenario**.
*   Varje request stämmer med **ert** API‑kontrakt (metod/endpoint/headers/payload).

***

## Extra scenarion (frivilligt)

*   Lägg till ett **edge case** (t.ex. whitespace‑namn → 400 med `errors.name`).
*   Lägg till ett **DELETE/PUT**‑scenario och välj passande lyckad statuskod (204/200).

***

### Avslutning

Vi går igenom arbetet **tillsammans kl. 15:30–16:00**:  
Varje par väljer ett scenario och förklarar **varför** koden är rätt och **var** beslutet togs i kedjan (JWT / Policy / Validering / Affärsregel / DB / Miljö).
