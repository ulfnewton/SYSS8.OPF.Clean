# Workshop: Bygg basen för Clean Architecture‑applikation

## Inledning

I denna workshop bygger du basen för en **Clean Architecture**‑applikation med:
*   **Domain** (entiteter/modeller)
  *  Inga beroenden till andra lager
*  **Application** (affärslogik, tjänster, interfaces)
  *  Beroende på Domain
*  **Infrastructure** (databas, externa tjänster)
  *  Beroende på Application och Domain
*  **WebApi** (REST API, DTO, endpoints)
  *  Beroende på Infrastructure, Application och Domain
*  **WebUi** (Blazor Server‑app)
  *  Anropar WebApi via HttpClient

Börja med att forka grund-repo:t för kursen: 
<https://github.com/ulfnewton/SYSS8.OPF.Clean>

Vi återanvänder den kod som vi har gått igenom i kursen under momentent codeAlong/workshop AuthorsAPI och UiStatus‑mönstret för Blazor.

> **Viktigt om branch:**  
> Efter att du forkat grund-repo, byt **omedelbart** till en **egen branch** innan du börjar:
>
> ```bash
> git checkout -b <ditt-namn>/workshop-base
> ```

***

## (Valfritt) A. Säkerställ rätt .NET‑SDK lokalt

Det här steget låser SDK‑versionen så att alla kör samma verktyg.

1.  Kolla vilken SDK du har:

```bash
dotnet --version
```

2.  Om din version **inte** matchar den vi använder i kursen (meddelas av läraren), skapa/uppdatera `global.json` **i repo‑roten** och lås till kursens version:

```bash
dotnet new globaljson --version <8.x.yyy> --force
```

> Om du redan har en `global.json` i repo:t behöver du inte göra något – den styr SDK‑versionen.  
> (Skulle du behöva uppdatera senare upprepar du kommandot ovan med ny version.)

***

## 1. Skapa lösning och projekt (exakta namn)

Kör i repo‑roten:

```bash
dotnet new sln -n SYSS8.OPF.Clean

dotnet new classlib -n SYSS8.OPF.Clean.Domain          -o src/SYSS8.OPF.Clean.Domain
dotnet new classlib -n SYSS8.OPF.Clean.Application     -o src/SYSS8.OPF.Clean.Application
dotnet new classlib -n SYSS8.OPF.Clean.Infrastructure  -o src/SYSS8.OPF.Clean.Infrastructure

dotnet new web     -n SYSS8.OPF.Clean.WebApi          -o src/SYSS8.OPF.Clean.WebApi
dotnet new blazor  --interactivity Server -n SYSS8.OPF.Clean.WebUi -o src/SYSS8.OPF.Clean.WebUi -f net8.0

dotnet sln add src/*/*.csproj
```

**Referenser mellan projekt (Clean):**

```bash
dotnet add src/SYSS8.OPF.Clean.Application      reference src/SYSS8.OPF.Clean.Domain
dotnet add src/SYSS8.OPF.Clean.Infrastructure   reference src/SYSS8.OPF.Clean.Application src/SYSS8.OPF.Clean.Domain
dotnet add src/SYSS8.OPF.Clean.WebApi           reference src/SYSS8.OPF.Clean.Infrastructure src/SYSS8.OPF.Clean.Application src/SYSS8.OPF.Clean.Domain
# WebUi anropar WebApi via HTTP → ingen referens till core-projekten behövs
```

***

## 2. Lägg paket med kursens versioner

**Infrastructure** (EF Core **8.0.23**):

```bash
dotnet add src/SYSS8.OPF.Clean.Infrastructure package Microsoft.EntityFrameworkCore --version 8.0.23
dotnet add src/SYSS8.OPF.Clean.Infrastructure package Microsoft.EntityFrameworkCore.InMemory --version 8.0.23
dotnet add src/SYSS8.OPF.Clean.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.23
```

**WebApi** (EF Core, OpenAPI, Swagger **6.6.2**):

```bash
dotnet add src/SYSS8.OPF.Clean.WebApi package Microsoft.EntityFrameworkCore --version 8.0.23
dotnet add src/SYSS8.OPF.Clean.WebApi package Microsoft.EntityFrameworkCore.InMemory --version 8.0.23
dotnet add src/SYSS8.OPF.Clean.WebApi package Microsoft.AspNetCore.OpenApi --version 8.0.23
dotnet add src/SYSS8.OPF.Clean.WebApi package Swashbuckle.AspNetCore --version 6.6.2
```

***

## 3. Domain – entiteter (endast modeller)

**Skapa fil:** `src/SYSS8.OPF.Clean.Domain/Author.cs`

> **Var hittar jag koden?**  
> Koden som ska klistras in finns i kursens repo (och i `docs/`), och motsvarande upplägg finns även i **Authors.API**-exemplet:  
> <https://github.com/ulfnewton/Authors.API> (titta på `Author`/`Book`‑modeller)

**Klistra in:**

```csharp
using System.Text.Json.Serialization;

namespace SYSS8.OPF.Clean.Domain
{
    public class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; } = new();
    }
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        // Navigation
        public Guid AuthorId { get; set; }
        [JsonIgnore]
        public Author Author { get; set; }
    }
}
```

> **OBS:** Inga **DTO** i Domain – de hör hemma i WebApi.

***

## 4. Infrastructure – DbContext

**Skapa fil:** `src/SYSS8.OPF.Clean.Infrastructure/AuthorDbContext.cs`

> **Var hittar jag koden?**  
> Denna följer samma mönster som i Authors.API.  
> <https://github.com/ulfnewton/Authors.API>

**Klistra in:**

```csharp
using Microsoft.EntityFrameworkCore;
using SYSS8.OPF.Clean.Domain;

namespace SYSS8.OPF.Clean.Infrastructure
{
    public class AuthorDbContext : DbContext
    {
        public AuthorDbContext(DbContextOptions builder) : base(builder) { }

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book>   Books   => Set<Book>();
    }
}
```

***

## 5. WebApi – DTO, endpoints, Program

### 5.1 DTO (WebApi)

**Skapa fil:** `src/SYSS8.OPF.Clean.WebApi/Contracts/Dtos.cs`

> **Var hittar jag koden?**  
> Finns i kursens repo och i Authors.API som mönster.  
> <https://github.com/ulfnewton/Authors.API>

**Klistra in:**

```csharp
namespace SYSS8.OPF.Clean.WebApi.Contracts
{
    public record AuthorDTO(string Name);
    public record BookDTO(string Title);
}
```

### 5.2 Endpoints

*   Lägg dina endpoints i:
    *   `src/SYSS8.OPF.Clean.WebApi/Endpoints/AuthorEndpoints.cs`
    *   `src/SYSS8.OPF.Clean.WebApi/Endpoints/MapEndpointsExtensions.cs`

> **Var hittar jag koden?**  
> Du har den i kursrepo:t under `src/SYSS8.OPF.Clean.WebApi/Endpoints/…`.  
> Samma mönster finns i **Authors.API**:  
> <https://github.com/ulfnewton/Authors.API>

Se till att:

*   `MapEndpointsExtensions.MapAuthorEndpoints` mappar **POST/GET/GET{id}/PUT{id}/DELETE{id}** för **authors**, samt **POST /authors/{authorId}/books**.
*   `MapEndpointsExtensions.MapBookEndpoints` mappar **GET/GET{id}/PUT{id}/DELETE{id}** för **books**.
*   `AuthorEndpoints` använder **AuthorDbContext** och **AuthorDTO/BookDTO** från **WebApi.Contracts**.

### 5.3 Program.cs (EF InMemory, Swagger, **CORS**, endpoints)

**Öppna:** `src/SYSS8.OPF.Clean.WebApi/Program.cs` och **lägg till**:

*   **EF InMemory**:
    ```csharp
    builder.Services.AddDbContext<AuthorDbContext>(
        options => options.UseInMemoryDatabase("authors"));
    ```
*   **Swagger**:
    ```csharp
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    ```
*   **CORS (Dev)**:
    ```csharp
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", p =>
            p.AllowAnyOrigin()     // DEV: kan bytas till WithOrigins("https://localhost:<ui-port>")
             .AllowAnyHeader()
             .AllowAnyMethod());
    });
    ```
*   **Pipeline**:
    ```csharp
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.Urls.Add("https://localhost:5001"); // vi kör API:t här i kursen
    app.UseHttpsRedirection();
    app.UseCors("DevCors");

    app.MapEndpoints(); // Mappa alla dina endpoints
    app.Run();
    ```

***

## 6. WebUi – Razor Components (server), IUiStatus, HttpClient

### 6.1 IUiStatus & UiStatus

Filerna ska ligga i `src/SYSS8.OPF.Clean.WebUi/Services/`:

*   `IUiStatus.cs`
*   `UiStatus.cs`

> **Var hittar jag koden?**  
> Du har dem redan i kursrepo:t; samma mönster finns i **blazor.uistatus**:  
> <https://github.com/ulfnewton/blazor.uistatus>  
> (Titta på `Services/IUiStatus.cs` och `Services/UiStatus.cs` – vi använder exakt samma idé.)

### 6.2 Registrera `IUiStatus` och **HttpClient** till WebApi

**Öppna:** `src/SYSS8.OPF.Clean.WebUi/Program.cs`

*   Registrera status‑tjänsten:
    ```csharp
    builder.Services.AddScoped<IUiStatus, UiStatus>();
    ```
*   Registrera en **namngiven HttpClient** till WebApi:
    ```csharp
    builder.Services.AddHttpClient("WebApi", c =>
    {
        c.BaseAddress = new Uri("https://localhost:5001");
    });
    ```

> `MainLayout.razor` använder redan `IUiStatus` för **Offline/Busy/Error** – bra, låt det vara.

***

## 7. Lägg in **http/authors.http** (testa CRUD)

Skapa mappen `http/` (om den saknas) och filen `http/authors.http` med **precis detta innehåll**:

```http
@baseUrl = https://localhost:5001
@json = application/json

###
# LISTA alla authors (förväntat: 200, array)
GET {{baseUrl}}/authors

### SKAPA author
# @name createAuthor
POST {{baseUrl}}/authors
Content-Type: {{json}}

{ "Name": "Douglas Adams" }

###
@id = {{createAuthor.response.body.$.id}}

###
# SKAPA ogiltig author (förväntat: 400)
POST {{baseUrl}}/authors
Content-Type: {{json}}

{ "Name": "   " }

###
# HÄMTA en av authors (kopiera ett Id från LISTA eller Location)
# Sätt variabeln själv när du testkör:
GET {{baseUrl}}/authors/{{id}}

###
# UPPDATERA author (förväntat: 200)
PUT {{baseUrl}}/authors/{{id}}
Content-Type: {{json}}

{ "Name": "Douglas Adams, Jr." }

###
# UPPDATERA tomt namn (förväntat: 400)
PUT {{baseUrl}}/authors/{{id}}
Content-Type: {{json}}

{ "Name": " " }

###
# UPPDATERA till ett namn som finns (förväntat: 409)
# Skapa först en annan author med samma namn,
# upprepa sedan PUT mot samma id med det namnet.
PUT {{baseUrl}}/authors/{{id}}
Content-Type: {{json}}

{ "Name": "Douglas Adams" }

###
# TA BORT author (förväntat: 204)
DELETE {{baseUrl}}/authors/{{id}}

###
# HÄMTA borttagen author (förväntat: 404)
GET {{baseUrl}}/authors/{{id}}
```

> Den här varianten sparar `id` från svaret vid **Create** och återanvänder den i resterande requests.

***

## 8. Kör & verifiera (DoD)

**Terminal A – WebApi**

```bash
dotnet run --project src/SYSS8.OPF.Clean.WebApi/SYSS8.OPF.Clean.WebApi.csproj
# Verifiera i terminalen att det lyssnar på https://localhost:5001
# Öppna https://localhost:5001/swagger (Development)
```

**Terminal B – WebUi**

```bash
dotnet run --project src/SYSS8.OPF.Clean.WebUi/SYSS8.OPF.Clean.WebUi.csproj
# Öppna applikationen i webbläsaren; inga CORS-fel ska synas i konsolen
```

**Kör http‑filen** `http/authors.http` i din editor (VS Code REST Client / Rider / Visual Studio Endpoints).  
**Godkänt när** du ser:

*   **201 + Location** på första **POST /authors**,
*   **400** vid tomt namn, **409** vid duplikat,
*   **200** vid **GET/PUT** (korrekt uppdaterat namn),
*   **204** vid **DELETE**, följt av **404** på GET av borttagen resurs.

***

## 9. Vanliga fel & fixar

*   **CORS‑fel:** Kontrollera att `app.UseCors("DevCors")` finns i WebApi efter `UseHttpsRedirection()` och att WebUi‑klienten pekar på **<https://localhost:5001>**.
*   **HTTPS‑certifikat lokalt:** Kör `dotnet dev-certs https --trust` om webbläsaren varnar.
*   **Fel paket/TFM:** Alla projekt ska vara **net8.0**; paketversioner **8.0.23** (EF Core) och **6.6.2** (Swashbuckle) där vi la till dem.

***

## 10. Commit & branch‑disciplin

1.  Lägg till och committa i rimliga steg:

```bash
git add .
git commit -m "Base: solution, projects, EF InMemory, CORS, HttpClient, authors.http"
```

2.  Pusha din branch:

```bash
git push --set-upstream origin <ditt-namn>/workshop-base
```

***

### Vidare läsning / var koden också finns

*   **UiStatus‑mönstret** (Services/IUiStatus.cs och Services/UiStatus.cs) finns också här:  
    <https://github.com/ulfnewton/blazor.uistatus>
*   **Authors/Books‑kontrakt + endpoints‑upplägg** (Author/Book, Minimal APIs):  
    <https://github.com/ulfnewton/Authors.API>

> I kursrepo:t (den branch du fick) finns **alla filer du behöver klistra in** också lokalt under `src/...` och `docs/...` – använd dem i första hand för att få **exakt samma version** som vi jobbar med i klassrummet.

***

Nu är du klar med basen för din Clean Architecture‑applikation!
