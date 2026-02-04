using SYSS8.OPF.Clean.WebApi.Dtos.Author;

namespace SYSS8.OPF.Clean.WebApi.Dtos.Books
{
    public record BookRequest(string Title);
    public record BookResponse(Guid Id, string Title);
    public record GetBookResponse(Guid Id, string Title, Guid? AuthorId = null);
    public record BookDetailsResponse(Guid Id, string Title, AuthorResponse? Author = null);
}
