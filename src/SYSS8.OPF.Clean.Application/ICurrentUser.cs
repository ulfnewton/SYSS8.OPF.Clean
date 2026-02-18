namespace SYSS8.OPF.Clean.WebApi.Identity
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? Email { get;  }
        IReadOnlyList<string> Roles { get; }
    }
}
