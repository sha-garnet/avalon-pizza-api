namespace AvalonPizza.Server.Attributes;

// This attribute can be placed on top of individual actions or whole Controllers
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ApiKeyRequiredAttribute : Attribute
{
}