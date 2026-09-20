using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels;

public class HistoricoPedidosPageModel : INotifyPropertyChanged
{
    private readonly HistoricoPedidosService _historicoPedidosService;
    private readonly PedidoViewModel _pedidoViewModel;

    public ObservableCollection<PedidoHistorico> PedidosHistoricos
    {
        get => _historicoPedidosService.Pedidos;
    }

    public ICommand VoltarCommand { get; }
    public ICommand RemoverPedidoCommand { get; }
    public ICommand EditPedidoCommand { get; }

    public HistoricoPedidosPageModel(HistoricoPedidosService historicoPedidosService, PedidoViewModel pedidoViewModel)
    {
        _historicoPedidosService = historicoPedidosService;
        _pedidoViewModel = pedidoViewModel;
        VoltarCommand = new Command(async () => await VoltarAsync());
        RemoverPedidoCommand = new Command<PedidoHistorico>(async pedido => await RemoverPedidoAsync(pedido));
        EditPedidoCommand = new Command<PedidoHistorico>(async pedido => await EditPedidoAsync(pedido));
    }

    public Task CarregarAsync()
    {
        return _historicoPedidosService.LoadAsync();
    }

    private async Task VoltarAsync()
    {
        await Shell.Current.Navigation.PopAsync();
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

    private async Task EditPedidoAsync(PedidoHistorico? pedido)
    {
        if (pedido is null)
        {
            return;
        }

        var confirmou = await Shell.Current.DisplayAlertAsync(
            "Editar este pedido?",
            $"Vais editar o pedido de {pedido.DataHora:dd/MM/yyyy HH:mm}. As quantidades atuais em 'Pedido de Bebidas' serão substituídas pelas deste pedido guardado. Continuar?",
            "Editar",
            "Cancelar");

        if (!confirmou)
        {
            return;
        }

        _pedidoViewModel.CarregarPedidoParaEdicao(pedido);
        await Shell.Current.Navigation.PushAsync(new Pages.PedidoPage(_pedidoViewModel));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}