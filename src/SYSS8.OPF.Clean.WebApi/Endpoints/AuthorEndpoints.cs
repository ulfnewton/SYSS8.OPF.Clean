using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SYSS8.OPF.Clean.Domain;
using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Contracts;
using SYSS8.OPF.Clean.WebApi.Identity;

namespace SYSS8.OPF.Clean.WebApi.Endpoints;

public static class AuthorEndpoints
{
    public static async Task<Results<Created<Author>, ProblemHttpResult>> CreateAuthor(AuthorDTO dto, AuthorDbContext context)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid author name",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Author name cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        var isDuplicate = await context.Authors.AnyAsync(
            author => author.Name.Equals(dto.Name, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            var pd = new ProblemDetails
            {
                Title = "Duplicate author",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author name '{dto.Name}' already exists"
            };

            return TypedResults.Problem(pd);
        }

        var author = new Author
        {
            Name = dto.Name,
        };

        await context.AddAsync(author);
        await context.SaveChangesAsync(CancellationToken.None);

        return TypedResults.Created($"/authors/{author.Id}", author);
    }

    public static async Task<Results<Ok<Author>, ProblemHttpResult>> UpdateAuthor(
        [FromRoute] Guid id,
        AuthorDTO dto,
        AuthorDbContext context,
        ICurrentUser current)
    {
        if (id == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid author name",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Author name cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        var author = await context.Authors.FindAsync(new object[] { id });

        if (author is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        // FIX: Här kontrollerar vi ifall det är ägaren eller en admin som genomför denna åtgärd.
        // Om inte, förbjud åtgärden!
        var isAdmin = current.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        if (!isAdmin && author.CreatedBy != current.UserId)
            return TypedResults.Problem(new ProblemDetails 
            {
                Title = "Forbidden",
                Detail = "You are not the owner or an admin, and are not allowed to update authors.",
                Status = 403, 
            });

        var isDuplicate = await context.Authors.AnyAsync(
            existing => existing.Id != id && existing.Name.Equals(dto.Name, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            var pd = new ProblemDetails
            {
                Title = "Duplicate author",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author name '{dto.Name}' already exists"
            };

            return TypedResults.Problem(pd);
        }

        author.Name = dto.Name;

        await context.SaveChangesAsync(CancellationToken.None);

        return TypedResults.Ok(author);
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteAuthor(
        [FromRoute] Guid id,
        AuthorDbContext context)
    {
        if (id == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        var author = await context.Authors.FindAsync(new object[] { id });

        if (author is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        var books = await context.Books.Where(book => book.AuthorId == id).ToListAsync();

        if (books.Count > 0)
        {
            context.Books.RemoveRange(books);
        }

        context.Authors.Remove(author);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<List<Author>>, ProblemHttpResult>> GetAuthors(AuthorDbContext context)
    {
        var authors = await context.Authors.ToListAsync();
        return TypedResults.Ok(authors);
    }

    public static async Task<Results<Ok<Author>, ProblemHttpResult>> GetAuthor(
        [FromRoute] Guid id,
        AuthorDbContext context,
        [FromQuery] bool includeBooks = false)
    {
        if (id == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        IQueryable<Author> author = context.Authors
                            .Where(a => a.Id == id);

        if (!await author.AnyAsync()) // alternativt: author.Count() == 0
        {
            var pd = new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{id}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        if (includeBooks)
        {
            author = author.Include(a => a.Books);     // Eager loading
        }

        return TypedResults.Ok(await author.FirstOrDefaultAsync());
    }

        public static async Task<Results<Ok<List<Book>>, ProblemHttpResult>> GetBooks(
            AuthorDbContext context,
            [FromQuery] Guid? authorId = null)
    {
            if (authorId.HasValue && authorId.Value == Guid.Empty)
            {
                var pd = new ProblemDetails
                {
                    Title = "Empty Id",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Id cannot be empty"
                };

                return TypedResults.Problem(pd);
            }

            if (authorId.HasValue)
            {
                var authorExists = await context.Authors.AnyAsync(author => author.Id == authorId.Value);

                if (!authorExists)
                {
                    var pd = new ProblemDetails
                    {
                        Title = "Author Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = $"Author with ID '{authorId}' is not found"
                    };

                    return TypedResults.Problem(pd);
                }
            }

            var booksQuery = context.Books.AsQueryable();

            if (authorId.HasValue)
            {
                booksQuery = booksQuery.Where(book => book.AuthorId == authorId.Value);
            }

            var books = await booksQuery.ToListAsync();

        return TypedResults.Ok(books);
    }

        public static async Task<Results<Ok<Book>, ProblemHttpResult>> GetBook(
            [FromRoute] Guid bookId,
            AuthorDbContext context)
    {
            if (bookId == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

            var book = await context.Books.FirstOrDefaultAsync(
                existing => existing.Id == bookId);

        if (book is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Book Not Found",
                Status = StatusCodes.Status404NotFound,
                    Detail = $"Book with ID '{bookId}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        return TypedResults.Ok(book);
    }

    public static async Task<Results<Created<Book>, ProblemHttpResult>> CreateBook([FromRoute] Guid authorId, BookDTO dto, AuthorDbContext context)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid Book Title",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Book title cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        var author = await context.Authors.FindAsync(new object[] { authorId });

        if (author is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Author Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Author with ID '{authorId}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        var isDuplicate = await context.Books.AnyAsync(
            book => book.AuthorId == authorId &&
                    book.Title.Equals(dto.Title, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            var pd = new ProblemDetails
            {
                Title = "Book Title Already Exists",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Author with ID '{author.Name}' has already written a book with the title '{dto.Title}'"
            };

            return TypedResults.Problem(pd);
        }

        var book = new Book
        {
            Title = dto.Title,
            AuthorId = authorId,
            Author = author
        };

        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();

        return TypedResults.Created($"/books/{book.Id}", book);
    }

        public static async Task<Results<Ok<Book>, ProblemHttpResult>> UpdateBook(
            [FromRoute] Guid bookId,
            BookDTO dto,
            AuthorDbContext context)
    {
            if (bookId == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            var pd = new ProblemDetails
            {
                Title = "Invalid Book Title",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Book title cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

            var book = await context.Books.FirstOrDefaultAsync(
                existing => existing.Id == bookId);

        if (book is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Book Not Found",
                Status = StatusCodes.Status404NotFound,
                    Detail = $"Book with ID '{bookId}' is not found"
            };

            return TypedResults.Problem(pd);
        }

            var isDuplicate = await context.Books.AnyAsync(
                existing => existing.AuthorId == book.AuthorId && existing.Id != bookId &&
                            existing.Title.Equals(dto.Title, StringComparison.InvariantCultureIgnoreCase));

        if (isDuplicate)
        {
            var pd = new ProblemDetails
            {
                Title = "Book Title Already Exists",
                Status = StatusCodes.Status409Conflict,
                    Detail = $"Author with ID '{book.AuthorId}' has already written a book with the title '{dto.Title}'"
            };

            return TypedResults.Problem(pd);
        }

        book.Title = dto.Title;
        await context.SaveChangesAsync();

        return TypedResults.Ok(book);
    }

        public static async Task<Results<NoContent, ProblemHttpResult>> DeleteBook(
            [FromRoute] Guid bookId,
            AuthorDbContext context)
    {
            if (bookId == Guid.Empty)
        {
            var pd = new ProblemDetails
            {
                Title = "Empty Id",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Id cannot be empty"
            };

            return TypedResults.Problem(pd);
        }

            var book = await context.Books.FirstOrDefaultAsync(
                existing => existing.Id == bookId);

        if (book is null)
        {
            var pd = new ProblemDetails
            {
                Title = "Book Not Found",
                Status = StatusCodes.Status404NotFound,
                    Detail = $"Book with ID '{bookId}' is not found"
            };

            return TypedResults.Problem(pd);
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
