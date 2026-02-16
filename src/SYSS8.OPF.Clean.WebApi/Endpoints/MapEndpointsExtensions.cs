using SYSS8.OPF.Clean.WebApi.Auth;

namespace SYSS8.OPF.Clean.WebApi.Endpoints;

public static class MapEndpointsExtensions
{
    // DESIGN-VAL: Minimal API ger ett kompakt sätt att visa routing och behörighet i kursen.
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        // INFO: Vi delar upp mappingen per resurs (authors/books) för tydlighet.
        MapAuthenticationEndpoints(app);
        MapAuthorEndpoints(app);
        MapBookEndpoints(app);
        return app;
    }

    // RÄTT: Minimal APIs binder body automatiskt för komplexa typer (FromBody valfritt).
    // FIX: Bytte namn från MapAuth → MapAuthenticationEndpoints för att bättre spegla innehållet
    // FIX: Ändrade private till public så att den kan användas i MapAuthEndpoints
    // FIX: Tog bort [FromServices] från UserManager och RoleManager, eftersom de
    // kommer att injiceras automatiskt av ASP.NET Core när de används som parametrar
    // i en endpoint-metod.
    private static IEndpointRouteBuilder MapAuthenticationEndpoints(IEndpointRouteBuilder app)
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
        return app;
    }

    private static void MapAuthorEndpoints(IEndpointRouteBuilder app)
    {
        // TIPS: RequireAuthorization kopplar policy-namn till roller i Program.cs.
        app.MapPost("/authors", AuthorEndpoints.CreateAuthor)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization("CanCreateAuthor")
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

        app.MapPut("/authors/{id:guid}", AuthorEndpoints.UpdateAuthor)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName(nameof(AuthorEndpoints.UpdateAuthor))
            .WithDescription("Updates an author.");

        app.MapDelete("/authors/{id:guid}", AuthorEndpoints.DeleteAuthor)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AuthorEndpoints.DeleteAuthor))
            .WithDescription("Deletes an author.");

        app.MapPost("/authors/{authorId:guid}/books", AuthorEndpoints.CreateBook)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            // OBS: Samma policy-name används för att koppla route till rollkrav.
            .RequireAuthorization("CanCreateAuthor")
            .WithName(nameof(AuthorEndpoints.CreateBook))
            .WithDescription("Creates a book from the given author.");
    }

    private static void MapBookEndpoints(IEndpointRouteBuilder app)
    {
        // DESIGN-VAL: Separat metod för books gör routing tydlig i undervisning.
        app.MapGet("/books", AuthorEndpoints.GetBooks)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AuthorEndpoints.GetBooks))
            .WithDescription("Gets all books.");

        app.MapGet("/books/{bookId:guid}", AuthorEndpoints.GetBook)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName(nameof(AuthorEndpoints.GetBook))
            .WithDescription("Gets a book by id.");

        app.MapPut("/books/{bookId:guid}", AuthorEndpoints.UpdateBook)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            // INFO: Policy-namn hålls stabila så att studenter kopplar rollkrav till endpoint.
            .RequireAuthorization("CanDeleteBook")
            .WithName(nameof(AuthorEndpoints.UpdateBook))
            .WithDescription("Updates a book by id.");

        app.MapDelete("/books/{bookId:guid}", AuthorEndpoints.DeleteBook)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("CanDeleteBook")
            .WithName(nameof(AuthorEndpoints.DeleteBook))
            .WithDescription("Deletes a book by id.");
    }
}