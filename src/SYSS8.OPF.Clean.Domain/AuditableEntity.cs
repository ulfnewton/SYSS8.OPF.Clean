namespace SYSS8.OPF.Clean.Domain
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid? LastModifiedBy { get; private set; }
        public DateTime? LastModifiedAtUtc { get; private set; }

        public void SetCreated(Guid userId, DateTime nowUtc)
            => (CreatedBy, CreatedAtUtc) = (userId, nowUtc);

        public void SetModified(Guid userId, DateTime nowUtc)
            => (LastModifiedBy, LastModifiedAtUtc) = (userId, nowUtc);
    }
}
