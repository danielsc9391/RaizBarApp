namespace RaizBarApp.Models;

public static class CategoriasBebidas
{
    public static IReadOnlyList<string> Todas { get; } = new[]
    {
        "Cerveja",
        "Vinho",
        "Refrigerante",
        "Destilados",
        "Outros"
    };
}
