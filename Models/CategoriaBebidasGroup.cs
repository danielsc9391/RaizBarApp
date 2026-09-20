using System.Collections.ObjectModel;

namespace RaizBarApp.Models;

public sealed class CategoriaBebidasGroup : ObservableCollection<Bebida>
{
    public string Nome { get; }

    public CategoriaBebidasGroup(string nome, IEnumerable<Bebida> bebidas)
        : base(bebidas)
    {
        Nome = nome;
    }
}
