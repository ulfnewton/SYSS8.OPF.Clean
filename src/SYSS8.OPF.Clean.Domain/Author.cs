using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
