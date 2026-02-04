using SYSS8.OPF.Clean.Domain;

namespace SYSS8.OPF.Clean.WebApi.Dtos.Author
{
    public record AuthorRequest(string Name);
    public record AuthorResponse(Guid Id, string Name);
    public record GetAuthorResponse(Guid Id, string Name, IEnumerable<Book>? Books = null);
}
