using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using SYSS8.OPF.Clean.Domain;

namespace SYSS8.OPF.Clean.Infrastructure
{
    public class AuthorDbContext : IdentityDbContext<User, Role, Guid>
    {
        public AuthorDbContext(DbContextOptions builder) : base(builder) { }

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
    }
    public class User : IdentityUser<Guid> { }
    public class Role : IdentityRole<Guid> { }
}
