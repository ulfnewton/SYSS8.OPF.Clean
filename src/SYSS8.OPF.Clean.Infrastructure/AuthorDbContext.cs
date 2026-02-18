using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


using SYSS8.OPF.Clean.Domain;
using SYSS8.OPF.Clean.WebApi.Identity;

namespace SYSS8.OPF.Clean.Infrastructure;

public class AuthorDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly ICurrentUser _current;

    public AuthorDbContext(DbContextOptions builder, ICurrentUser current) : base(builder) 
    {
        _current = current;
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    // OBS: Vi måste implementera båda overload-metoderna annars kan det hända att vi anropar base.SaveChangesAsync
    public async Task<int> SaveChangesAsync() 
        => await SaveChangesAsync(acceptAllChangesOnSuccess: true, CancellationToken.None);

    // OBS: Vi måste implementera båda overload-metoderna annars kan det hända att vi anropar base.SaveChangesAsync
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
                => await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    // OBS: Vi måste implementera båda overload-metoderna annars kan det hända att vi anropar base.SaveChangesAsync
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var userId = _current.UserId ?? Guid.Empty;

        foreach (var entity in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entity.State == EntityState.Added)
            {
                entity.Entity.SetCreated(userId, now);
            }

            if (entity.State == EntityState.Modified)
            {
                entity.Entity.SetModified(userId, now);
            }
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }
}
