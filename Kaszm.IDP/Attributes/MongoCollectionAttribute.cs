namespace IdentityServer.Attributes;

public class MongoCollectionAttribute : Attribute
{
    public string Name { get; private set; }

    public MongoCollectionAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("collection name cannot be empty");
        }
        Name = name;
    }
}