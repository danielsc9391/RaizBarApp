using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels;

public class HistoricoPedidosPageModel : INotifyPropertyChanged
{
    private ObservableCollection<PedidoHistorico> _pedidosHistoricos = new ObservableCollection<PedidoHistorico>();

    public ObservableCollection<PedidoHistorico> PedidosHistoricos
    {
        get => _pedidosHistoricos;
        set
        {
            if (_pedidosHistoricos != value)
            {
                _pedidosHistoricos = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand VoltarCommand { get; }

    public HistoricoPedidosPageModel()
    {
        VoltarCommand = new Command(async () => await VoltarAsync());
        
        // Load historical orders
        LoadHistoricoPedidos();
    }

    private void LoadHistoricoPedidos()
    {
        // Clear current list
        PedidosHistoricos.Clear();
        
        // Add all historical orders from the static property in PedidoViewModel
        foreach (var pedido in PedidoViewModel.HistoricoPedidos)
        {
            PedidosHistoricos.Add(pedido);
        }
    }

    private async Task VoltarAsync()
    {
        await Application.Current.MainPage.Navigation.PopAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}