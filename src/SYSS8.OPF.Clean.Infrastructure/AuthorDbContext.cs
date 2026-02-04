using Microsoft.EntityFrameworkCore;

using SYSS8.OPF.Clean.Domain;

namespace SYSS8.OPF.Clean.Infrastructure
{
    public class AuthorDbContext : DbContext
    {
        public AuthorDbContext(DbContextOptions builder) : base(builder) { }

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
    }
}
