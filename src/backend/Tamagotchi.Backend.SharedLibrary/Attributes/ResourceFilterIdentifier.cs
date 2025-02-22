namespace Tamagotchi.Backend.SharedLibrary.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ResourceFilterIdentifier : Attribute
{
    public string ResourceIdentifier { get; }

    public ResourceFilterIdentifier(string resourceIdentifier)
    {
        ResourceIdentifier = resourceIdentifier;
    }
}
