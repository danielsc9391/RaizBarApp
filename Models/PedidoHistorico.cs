using System.ComponentModel;

namespace RaizBarApp.Models;

public class PedidoHistorico : INotifyPropertyChanged
{
    public DateTime DataHora { get; set; }
    public List<Bebida> Bebidas { get; set; } = new List<Bebida>();
    public decimal Total { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}