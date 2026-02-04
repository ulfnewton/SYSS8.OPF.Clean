using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SYSS8.OPF.Clean.Domain;
using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Dtos.Author;
using SYSS8.OPF.Clean.WebApi.Dtos.Books;

namespace SYSS8.OPF.Clean.WebApi.Endpoints;

public static class AuthorEndpoints
{
    // ---------- AUTHORS ----------

    public static async Task<Results<Created<AuthorResponse>, ProblemHttpResult>> CreateAuthor(
        AuthorRequest dto,
        AuthorDbContext context)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Invalid author name",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Author name cannot be empty"
            });
        }

        var isDuplicate = await context.Authors
            .AnyAsync(a => a.Name.Equals(dto.Name, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Duplicate author",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author name '{dto.Name}' already exists"
            });
        }

        var author = new Author { Name = dto.Name.Trim() };
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Kort 201-payload + Location (Swagger-vänligt kontrakt)
        return TypedResults.Created($"/authors/{author.Id}", new AuthorResponse(author.Id, author.Name));
    }

    public static async Task<Results<Ok<AuthorResponse>, ProblemHttpResult>> UpdateAuthor(
        [FromRoute] Guid id,
        AuthorRequest dto,
        AuthorDbContext context)
    {
        if (id == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Invalid author name",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Author name cannot be empty"
            });
        }

        var author = await context.Authors.FindAsync(id);
        if (author is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            });
        }

        var isDuplicate = await context.Authors
            .AnyAsync(a => a.Id != id && a.Name.Equals(dto.Name, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Duplicate author",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author name '{dto.Name}' already exists"
            });
        }

        author.Name = dto.Name.Trim();
        await context.SaveChangesAsync();

        return TypedResults.Ok(new AuthorResponse(author.Id, author.Name));
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteAuthor(
        [FromRoute] Guid id,
        AuthorDbContext context)
    {
        if (id == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        var author = await context.Authors.FindAsync(id);
        if (author is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            });
        }

        // Restrict-delete: förhindra radering om relationer finns (409)
        var hasBooks = await context.Books.AnyAsync(b => b.AuthorId == id);
        if (hasBooks)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Delete restricted",
                Status = StatusCodes.Status409Conflict,
                Detail = "Author cannot be deleted because related books exist"
            });
        }

        context.Authors.Remove(author);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<Ok<IEnumerable<GetAuthorResponse>>> GetAuthors(AuthorDbContext context)
    {
        var authors = await context.Authors
            .AsNoTracking()
            .Select(a => new GetAuthorResponse(a.Id, a.Name, null))
            .ToListAsync();

        return TypedResults.Ok(authors.AsEnumerable());
    }

    public static async Task<Results<Ok<GetAuthorResponse>, ProblemHttpResult>> GetAuthor(
        [FromRoute] Guid id,
        AuthorDbContext context,
        [FromQuery] bool includeBooks = false)
    {
        if (id == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        IQueryable<Author> q = context.Authors.AsNoTracking().Where(a => a.Id == id);
        if (includeBooks) q = q.Include(a => a.Books);

        var a = await q.FirstOrDefaultAsync();
        if (a is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            });
        }

        var dto = new GetAuthorResponse(a.Id, a.Name, includeBooks ? a.Books : null);

        return TypedResults.Ok(dto);
    }

    // ---------- BOOKS ----------

    public static async Task<Results<Ok<IEnumerable<GetBookResponse>>, ProblemHttpResult>> GetBooks(
        AuthorDbContext context,
        [FromQuery] Guid? authorId = null,
        [FromQuery] bool includeAuthor = false)
    {
        if (authorId.HasValue && authorId.Value == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        IQueryable<Book> q = context.Books.AsNoTracking();

        if (authorId.HasValue)
        {
            var authorExists = await context.Authors.AnyAsync(a => a.Id == authorId.Value);
            if (!authorExists)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Author Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Author with ID '{authorId}' is not found"
                });
            }
            q = q.Where(b => b.AuthorId == authorId.Value);
        }

        if (includeAuthor) q = q.Include(b => b.Author);

        List<GetBookResponse> result;

        if (includeAuthor)
        {
            result = await q.Select(b => new GetBookResponse(b.Id, b.Title, b.AuthorId)).ToListAsync();
        }
        else
        {
            result = await q.Select(b => new GetBookResponse(b.Id, b.Title, null)).ToListAsync();
        }

        return TypedResults.Ok(result.AsEnumerable());
    }

    public static async Task<Results<Ok<BookDetailsResponse>, ProblemHttpResult>> GetBook(
        [FromRoute] Guid bookId,
        AuthorDbContext context,
        [FromQuery] bool includeAuthor = false)
    {
        if (bookId == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        IQueryable<Book> q = context.Books.AsNoTracking().Where(b => b.Id == bookId);
        if (includeAuthor) q = q.Include(b => b.Author);

        var b = await q.FirstOrDefaultAsync();
        if (b is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Book Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Book with ID '{bookId}' is not found"
            });
        }

        var dto = includeAuthor && b.Author is not null
            ? new BookDetailsResponse(b.Id, b.Title, new AuthorResponse(b.Author.Id, b.Author.Name))
            : new BookDetailsResponse(b.Id, b.Title);

        return TypedResults.Ok(dto);
    }

    // Route-ägt id (smal DTO), kort 201-svar + Location
    public static async Task<Results<Created<BookResponse>, ProblemHttpResult>> CreateBook(
        [FromRoute] Guid id,
        BookRequest dto,
        AuthorDbContext context)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Invalid Book Title",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Book title cannot be empty"
            });
        }

        var author = await context.Authors.FindAsync(id);
        if (author is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            });
        }

        var duplicate = await context.Books.AnyAsync(b =>
            b.AuthorId == id &&
            b.Title.Equals(dto.Title, StringComparison.InvariantCultureIgnoreCase));

        if (duplicate)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Book Title Already Exists",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author '{author.Name}' already has a book titled '{dto.Title}'"
            });
        }

        var book = new Book { Title = dto.Title.Trim(), AuthorId = id };
        context.Books.Add(book);
        await context.SaveChangesAsync();

        return TypedResults.Created($"/books/{book.Id}", new BookResponse(book.Id, book.Title));
    }

    public static async Task<Results<Ok<BookResponse>, ProblemHttpResult>> UpdateBook(
        [FromRoute] Guid bookId,
        BookRequest dto,
        AuthorDbContext context)
    {
        if (bookId == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Invalid Book Title",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Book title cannot be empty"
            });
        }

        var book = await context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        if (book is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Book Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Book with ID '{bookId}' is not found"
            });
        }

        var isDuplicate = await context.Books.AnyAsync(existing =>
            existing.AuthorId == book.AuthorId &&
            existing.Id != bookId &&
            existing.Title.Equals(dto.Title, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Book Title Already Exists",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author with ID '{book.AuthorId}' has already written a book with the title '{dto.Title}'"
            });
        }

        book.Title = dto.Title.Trim();
        await context.SaveChangesAsync();

        return TypedResults.Ok(new BookResponse(book.Id, book.Title));
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteBook(
        [FromRoute] Guid bookId,
        AuthorDbContext context)
    {
        if (bookId == Guid.Empty)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            });
        }

        var book = await context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        if (book is null)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title = "Book Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Book with ID '{bookId}' is not found"
            });
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
