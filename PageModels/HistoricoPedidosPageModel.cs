using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels;

public class HistoricoPedidosPageModel : INotifyPropertyChanged
{
    private readonly HistoricoPedidosService _historicoPedidosService;

    public ObservableCollection<PedidoHistorico> PedidosHistoricos
    {
        get => _historicoPedidosService.Pedidos;
    }

    public ICommand VoltarCommand { get; }
    public ICommand RemoverPedidoCommand { get; }

    public HistoricoPedidosPageModel(HistoricoPedidosService historicoPedidosService)
    {
        _historicoPedidosService = historicoPedidosService;
        VoltarCommand = new Command(async () => await VoltarAsync());
        RemoverPedidoCommand = new Command<PedidoHistorico>(async pedido => await RemoverPedidoAsync(pedido));
    }

    public Task CarregarAsync()
    {
        return _historicoPedidosService.LoadAsync();
    }

    private async Task VoltarAsync()
    {
        await Application.Current.MainPage.Navigation.PopAsync();
    }

    private async Task RemoverPedidoAsync(PedidoHistorico? pedido)
    {
        if (pedido is null)
        {
            return;
        }

        var confirmou = await Shell.Current.DisplayAlertAsync(
            "Eliminar pedido",
            "Eliminar este pedido do histórico?",
            "Eliminar",
            "Cancelar");

        if (confirmou)
        {
            await _historicoPedidosService.RemoveAsync(pedido);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}