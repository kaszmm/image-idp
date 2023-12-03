namespace IdentityServer;

public interface IConcurrencyAware
{ 
    string ConcurrencyTimestamp { get; set; }
}