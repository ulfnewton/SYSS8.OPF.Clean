# Workshop – Databassäkerhet – 2026‑02‑16

Arbeta i branchen [**`develop/v7/03-ui-login-integration`**](https://github.com/ulfnewton/SYSS8.OPF.Clean/tree/develop/v7/03-ui-login-integration). Visual Studio Community är utgångsläge. För Rider finns separata `.http`‑filer i avsnitt 8B.
***

## 0) Förutsättningar (engångs)

*   .NET SDK **8.0.417** (styrs av `global.json` i repo).
*   EF‑verktyg globalt:
    ```bash
    dotnet tool install --global dotnet-ef
    ```

> **Varför?** EF‑verktygen krävs för `dotnet ef migrations ...`, och projektet bygger mot `net8.0`.

***

## 1) Gå till rätt katalog

Kör alla CLI‑kommandon från **startup‑projektet** (WebApi):

```bash
cd src/SYSS8.OPF.Clean.WebApi
```

> **Varför?** Då fungerar `--startup-project .` och relativvägen till Infrastructure (`../SYSS8.OPF.Clean.Infrastructure`).

***

## 2) Paket – lås EF Core till 8.0.23

Installera **exakt** 8.0.23 (EF 10.x kräver .NET 10 och är inkompatibelt med `net8.0`):

**Infrastructure** (där `AuthorDbContext` finns):

```bash
dotnet add ../SYSS8.OPF.Clean.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.23
```

**WebApi** (startup‑projektet):

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.23
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.23
```

> **Varför?**  
> – `Sqlite` behövs i **båda** projekten (design‑tid och runtime).  
> – `Design` måste finnas i **startup‑projektet** för att EF‑verktygen ska fungera.

***

## 3) User Secrets – håll hemligheter utanför repo

Initiera och sätt kopplingssträng + JWT‑värden:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Data Source=dev.db"
dotnet user-secrets set "Jwt:Issuer" "syss8.dev"
dotnet user-secrets set "Jwt:Audience" "syss8.dev"
dotnet user-secrets set "Jwt:Key" "dev-secret-key-change-in-prod-32-bytes-minimum"
```

> **Notera:** `dotnet user-secrets init` skriver in `UserSecretsId` i WebApi‑projektets `.csproj`.

***

## 4) Växla från InMemory till SQLite

I `Program.cs` (WebApi), ersätt InMemory‑raden med `UseSqlite`:

```csharp
// Före (InMemory):
// builder.Services.AddDbContext<AuthorDbContext>(o => o.UseInMemoryDatabase("authors"));

// Efter (SQLite):
builder.Services.AddDbContext<AuthorDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
```

> Detta byte är förberett i diffen och kräver paketet `Microsoft.EntityFrameworkCore.Sqlite`.

***

## 5) Bygg rent

```bash
dotnet clean
dotnet restore
dotnet build
```

> Säkerställ att alla referenser pekar på EF **8.0.23** innan migrering.

***

## 6) Skapa och applicera migration

Skapa migration i **Infrastructure**, kör den med **WebApi** som startup:

```bash
dotnet ef migrations add Initial --project ../SYSS8.OPF.Clean.Infrastructure --startup-project .
dotnet ef database update --project ../SYSS8.OPF.Clean.Infrastructure --startup-project .
```

> Detta skapar `Migrations/*Initial*` i Infrastructure och bygger upp tabellerna (Authors, Books samt Identity‑tabeller) i `dev.db`. Migrationsfiler och snapshot i diffen bekräftar strukturen.

***

## 7) Starta API och kontrollera seeding

```bash
dotnet run
```

Vid start:

*   Roller skapas: **Admin**, **Lärare**, **Student**.
*   Demokonton skapas: `admin@example.com` (Admin), `teacher@example.com` (Lärare), `student@example.com` (Student); lösenord `Password1!`. 

> Basadressen `https://localhost:5001` läggs till i Program och används nedan i `.http`‑filerna. 

***

## 8) Testa med **`.http`** (förstahandsval)

Skapa filerna i **`src/SYSS8.OPF.Clean.WebApi/`**.  
Logik för inloggning (token, email, role) finns i `AuthEndpoints.Login`; policies **CanCreateAuthor/CanDeleteBook** konfigureras i `Program.cs`. 

### 8A) VS/VS Code – `.http` (namngiven request + referens)

**`auth.http`**

```http
@baseUrl = https://localhost:5001

### LOGIN – Lärare (namngiven request)
# @name login_vs
POST {{baseUrl}}/auth/login
Content-Type: application/json

{ "email": "teacher@example.com", "password": "Password1!" }
```

**`security_baslinje.http`**

```http
@baseUrl = https://localhost:5001

### 401 – utan token
POST {{baseUrl}}/authors
Content-Type: application/json

{ "name": "Demo" }

### 403 – Student saknar policy CanCreateAuthor
# @name login_student_vs
POST {{baseUrl}}/auth/login
Content-Type: application/json

{ "email": "student@example.com", "password": "Password1!" }

POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{login_student_vs.response.body.$.token}}

{ "name": "Demo" }

### 2xx – Lärare har policy CanCreateAuthor
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{login_vs.response.body.$.token}}

{ "name": "Demo-OK" }

### Öppna GET – kräver ingen token
GET {{baseUrl}}/authors
GET {{baseUrl}}/books
```

**`security_kontrakt.http`**

```http
@baseUrl = https://localhost:5001

### 400 – ogiltig modell (tomt namn)
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{login_vs.response.body.$.token}}

{ "name": "" }

### 404 – okänt id
GET {{baseUrl}}/authors/00000000-0000-0000-0000-000000000001

### 409 – dubblett
# 1) Skapa
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{login_vs.response.body.$.token}}

{ "name": "Unik" }

# 2) Skapa samma igen -> 409
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{login_vs.response.body.$.token}}

{ "name": "Unik" }
```

### 8B) Rider – separata `.rider.http` (globala variabler)

**`auth.rider.http`**

```http
@baseUrl = https://localhost:5001

### LOGIN – Lärare (sparar token som {{auth_token}})
# @name login_teacher_rider
POST {{baseUrl}}/auth/login
Content-Type: application/json

{ "email": "teacher@example.com", "password": "Password1!" }

> {% client.global.set("auth_token", response.body.token); %}

### LOGIN – Student (sparar token som {{auth_token_student}})
# @name login_student_rider
POST {{baseUrl}}/auth/login
Content-Type: application/json

{ "email": "student@example.com", "password": "Password1!" }

> {% client.global.set("auth_token_student", response.body.token); %}

### (Valfritt) LOGIN – Admin (sparar token som {{auth_token_admin}})
# @name login_admin_rider
POST {{baseUrl}}/auth/login
Content-Type: application/json

{ "email": "admin@example.com", "password": "Password1!" }

> {% client.global.set("auth_token_admin", response.body.token); %}
```

**`security_baslinje.rider.http`**

```http
@baseUrl = https://localhost:5001

### 401 – utan token
POST {{baseUrl}}/authors
Content-Type: application/json

{ "name": "Demo" }

### 403 – Student saknar policy CanCreateAuthor
# Kör login_student_rider i auth.rider.http först
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{auth_token_student}}

{ "name": "Demo" }

### 2xx – Lärare har policy CanCreateAuthor
# Kör login_teacher_rider i auth.rider.http först
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{auth_token}}

{ "name": "Demo-OK" }

### Öppna GET – kräver ingen token
GET {{baseUrl}}/authors
GET {{baseUrl}}/books
```

**`security_kontrakt.rider.http`**

```http
@baseUrl = https://localhost:5001

### 400 – ogiltig modell (tomt namn)
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{auth_token}}

{ "name": "" }

### 404 – okänt id
GET {{baseUrl}}/authors/00000000-0000-0000-0000-000000000001

### 409 – dubblett
# 1) Skapa "Unik"
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{auth_token}}

{ "name": "Unik" }

# 2) Skapa "Unik" igen -> konflikt
POST {{baseUrl}}/authors
Content-Type: application/json
Authorization: Bearer {{auth_token}}

{ "name": "Unik" }
```

> **Underlag i koden:** Inloggning returnerar `token`, `email`, `role` (AuthEndpoints + JwtTokenService); policies **CanCreateAuthor**/**CanDeleteBook** mappas i `Program.cs`. 

***

## 9) Vanliga fel och precisa åtgärder

*   **`Could not find project or directory '../SYSS8.OPF.Clean.Infrastructure'`**  
    Kör kommandon **inifrån** `src/SYSS8.OPF.Clean.WebApi`.

*   **`NU1202 … EFCore 10.x is not compatible with net8.0`**  
    Lås EF‑paket till **8.0.23** i samtliga projekt (se steg 2).

*   **`Your startup project doesn't reference Microsoft.EntityFrameworkCore.Design`**  
    Installera `Microsoft.EntityFrameworkCore.Design --version 8.0.23` i **WebApi** (startup).

*   **`Microsoft.EntityFrameworkCore.Sqlite` saknas**  
    Säkerställ `Microsoft.EntityFrameworkCore.Sqlite --version 8.0.23` i **WebApi** och **Infrastructure** och att `Program.cs` använder `UseSqlite(...)` (steg 4). Diffen visar exakt ändring och paket.

*   **Migration skapad men `dev.db` uppdateras inte**  
    Kör `dotnet ef ... --startup-project .` från **WebApi** och säkerställ **UserSecretsId** i WebApi‑projektets `.csproj` (sattes av `user-secrets init`).

***

## 10) “Allt‑i‑ett” (klipp & kör)

```bash
# Från repo-rot:
cd src/SYSS8.OPF.Clean.WebApi

# Paket
dotnet add ../SYSS8.OPF.Clean.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.23
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.23
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.23

# Hemligheter
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Data Source=dev.db"
dotnet user-secrets set "Jwt:Issuer" "syss8.dev"
dotnet user-secrets set "Jwt:Audience" "syss8.dev"
dotnet user-secrets set "Jwt:Key" "dev-secret-key-change-in-prod-32-bytes-minimum"

# Bygg
dotnet clean && dotnet restore && dotnet build

# Migration + update
dotnet ef migrations add Initial --project ../SYSS8.OPF.Clean.Infrastructure --startup-project .
dotnet ef database update --project ../SYSS8.OPF.Clean.Infrastructure --startup-project .

# Kör API
dotnet run
```

> **Kom ihåg:** `Program.cs` måste använda `UseSqlite(...)`. Diffen visar även att `UserSecretsId` finns i WebApi‑csproj.

***

## 11) Kodstöd – var syns vad?

*   **Auth & loginrespons (token, email, role)**: `AuthEndpoints.Login` + `JwtTokenService`. 
*   **Policy‑mappning**: `Program.cs` → `AddAuthorization` (CanCreateAuthor: Admin/Lärare; CanDeleteBook: Admin). 
*   **Bas‑URL och middleware**: `Program.cs` (Swagger, CORS, AuthN/AuthZ, `app.Urls.Add("https://localhost:5001")`). 
*   **SQLite‑växling + paket i WebApi**: diffen (UseSqlite, PackageReference, UserSecretsId).
*   **Seeding av roller/användare**: `IdentitySeeder` (Admin, Lärare, Student + demokonton). 
*   **Migrationsartefakter**: `Migrations/*Initial*` (Designer/Up/Down/Snapshot) i Infrastructure.

***

## 12) “Nollställ och gör om”

```bash
# Stoppa körning. Från repo-rot:
git checkout -- src/SYSS8.OPF.Clean.WebApi/Program.cs
git clean -fdx src/SYSS8.OPF.Clean.WebApi/   # rensa bin/obj/dev.db om den ligger här
rm -rf src/SYSS8.OPF.Clean.Infrastructure/Migrations

# Upprepa steg 1–10.
```

***

## 13) Miljöprofil för **Test**

I `Properties/launchSettings.json` (WebApi), lägg till:

```json
"test": {
  "commandName": "Project",
  "launchBrowser": true,
  "applicationUrl": "https://localhost:5003;http://localhost:5023",
  "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Test" }
}
```

> Separat `test.db` och secrets för **Test** sätter ni senare. Development fortsätter använda `dev.db`.

***

## Appendix A – curl (sekundärt)

```bash
# Login
curl -s -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"teacher@example.com","password":"Password1!"}'

# Skapa författare med token
TOKEN="<klistra_in_token>"
curl -i -s -X POST https://localhost:5001/authors \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Astrid Lindgren"}'
```

***

## Varför detta är databassäkerhet

*   **Miljöseparation** (Development/Test) med hemligheter i **User Secrets**—inget känsligt i repo.
*   **Kontrollerade migreringar** mot **SQLite** i Development—realistisk, deterministisk datalager‑setup före nästa miljö.
*   **Roll/policy‑styrning** kring skyddade endpoints; `.http`‑filerna demonstrerar 401/403/2xx‑mönster och felkontrakt. 
