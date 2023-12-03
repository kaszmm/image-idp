namespace IdentityServer.Models;

public record BaseEntity : IConcurrencyAware
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public string ConcurrencyTimestamp { get; set; }
}