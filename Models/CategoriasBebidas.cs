namespace RaizBarApp.Models;

public static class CategoriasBebidas
{
    public static IReadOnlyList<string> Todas { get; } = new[]
    {
        "Água",
        "Refrigerante",
        "Álcool",
        "Outros"
    };
}
