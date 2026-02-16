
namespace SYSS8.OPF.Clean.WebApi.Endpoints;

public static class MapEndpointsExtensions
{

    // DESIGN-VAL: Central plats för att mappa alla endpoints.
    // FIX: Själva authentication-mappningen flyttades hit från
    //      AuthenticationEndpoints.MapAuthEndpoints() för att samla all auth-logik 
    //      (register/login/me) i en och samma klass.
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        // Eftersom vi har flyttat MapAuthEndppoints från AuthEndpoints.cs till den här
        // klassen, så behöver vi inte längre anropa MapAuthEndpoints som en extension
        // method på app.MapAuthEndpoints().
        // FEL: app.MapAuthEndpoints();
        MapAuthEndpoints(app);
        MapAuthorEndpoints(app);
        MapBookEndpoints(app);
        return app;
    }

    // FIX: Flyttade från AuthEndpoints.cs, vilket gör att den inte längre är en extension method på IEndpointRouteBuilder, utan istället en vanlig statisk metod
    // som tar IEndpointRouteBuilder som parameter. Detta är mer passande eftersom MapAuthEndpoints inte längre är en del av AuthEndpoints-klassen, och det gör det tydligare att den är en del av den övergripande endpoint-mappningen i MapEndpointsExtensions.
    private static IEndpointRouteBuilder MapAuthEndpoints(IEndpointRouteBuilder app)
    {
        // DESIGN-VAL: MapGroup ger prefix + taggar och synlighet i Swagger under /auth.
        // I det här fallet lägger vi till en tag "Auth" på gruppen, vilket kan vara användbart
        // för dokumentation och verktyg som Swagger/OpenAPI för att kategorisera endpoints.
        var group = app.MapGroup("/auth").WithTags("Auth");
        group.MapPost("/register", AuthenticationEndpoints.Register);
        group.MapPost("/login", AuthenticationEndpoints.Login);

        // Design-val: Lägger till en "me" endpoint som kräver autentisering, för att demonstrera
        // hur man kan hämta information om den inloggade användaren.
        group.MapGet("/me", AuthenticationEndpoints.Me)
             .RequireAuthorization();


        // Lägg en prov-endpoint som kräver exakt samma policy
        app.MapGet("/authz/probe", () => Results.Ok("OK"))
           .RequireAuthorization("CanCreateAuthor");

        app.MapGet("/diag/authn-ok", () => Results.Ok("AuthN OK"))
           .RequireAuthorization();
        return app;
    }

    private static void MapAuthorEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/authors", AuthorEndpoints.CreateAuthor)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("CanCreateAuthor")            // Add Authorization
            .WithName(nameof(AuthorEndpoints.CreateAuthor))
            .WithDescription("Creates an author.");

        app.MapGet("/authors", AuthorEndpoints.GetAuthors)
            .Produces(StatusCodes.Status200OK)
            .WithName(nameof(AuthorEndpoints.GetAuthors))
            .WithDescription("Gets all authors.");

        app.MapGet("/authors/{id:guid}", AuthorEndpoints.GetAuthor)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AuthorEndpoints.GetAuthor))
            .WithDescription("Gets an author by id.");


        // DESIGN-VAL: Vi återanvänder “CanCreateAuthor” även för Update i kursen (förenklar matrisen).
        // Om du vill använda en separat policy kan du definiera den i Program.cs och
        // sedan referera till den här. 
        app.MapPut("/authors/{id:guid}", AuthorEndpoints.UpdateAuthor)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("CanCreateAuthor")            // Add Authorization
            .WithName(nameof(AuthorEndpoints.UpdateAuthor))
            .WithDescription("Updates an author.");

        app.MapDelete("/authors/{id:guid}", AuthorEndpoints.DeleteAuthor)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("CanDeleteAuthor")            // Add Authorization
            .WithName(nameof(AuthorEndpoints.DeleteAuthor))
            .WithDescription("Deletes an author.");

        app.MapPost("/authors/{authorId:guid}/books", AuthorEndpoints.CreateBook)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("CanCreateBook")              // Add Authorization
            .WithName(nameof(AuthorEndpoints.CreateBook))
            .WithDescription("Creates a book from the given author.");
    }

    private static void MapBookEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/books", AuthorEndpoints.GetBooks)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AuthorEndpoints.GetBooks))
            .WithDescription("Gets all books. Optionally filter by author using the authorId query parameter.");

        app.MapGet("/books/{bookId:guid}", AuthorEndpoints.GetBook)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AuthorEndpoints.GetBook))
            .WithDescription("Gets a book by id.");

        // FIX: Lägger till auktorisering för att uppdatera en book, kräver att användaren
        // har "CanCreateBook"-policyn (vilket i sin tur kräver att de har rollen "Admin"
        // eller "Lärare") - samma som för att skapa en book, eftersom det är samma
        // behörighetskrav.
        app.MapPut("/books/{bookId:guid}", AuthorEndpoints.UpdateBook)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("CanCreateBook")              // Add Authorization
            .WithName(nameof(AuthorEndpoints.UpdateBook))
            .WithDescription("Updates a book by id.");

        // FIX: Lägger till auktorisering för att ta bort en book, kräver att användaren
        // har "CanDeleteBook"-policyn (vilket i sin tur kräver att de har rollen "Admin")
        // - samma som för att ta bort en author, eftersom det är samma behörighetskrav.
        app.MapDelete("/books/{bookId:guid}", AuthorEndpoints.DeleteBook)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("CanDeleteBook")              // Add Authorization
            .WithName(nameof(AuthorEndpoints.DeleteBook))
            .WithDescription("Deletes a book by id.");
    }
}
