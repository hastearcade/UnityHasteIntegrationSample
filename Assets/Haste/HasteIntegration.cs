using System;

public sealed class HasteIntegration
{
    private static readonly Lazy<HasteIntegration> lazy =
        new Lazy<HasteIntegration>(() => new HasteIntegration());

    public static HasteIntegration Instance { get { return lazy.Value; } }

    public HasteClientIntegration Client { get; set; }
    public HasteServerIntegration Server { get; set; }

    private HasteIntegration()
    {
        Client = new HasteClientIntegration();
        Server = new HasteServerIntegration();
    }
}
