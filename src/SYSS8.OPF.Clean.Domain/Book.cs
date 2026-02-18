using System.Text.Json.Serialization;

namespace SYSS8.OPF.Clean.Domain
{
    public class Book : AuditableEntity
    {
        public string Title { get; set; } = "";
        public Guid AuthorId { get; set; }
        [JsonIgnore]
        public Author Author { get; set; } = null!;
    }
}
