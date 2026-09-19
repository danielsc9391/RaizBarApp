using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels;

public class PedidoViewModel : INotifyPropertyChanged
{
    private readonly BebidasService _bebidasService;
    private readonly HistoricoPedidosService _historicoPedidosService;
    private decimal _total;

    public ObservableCollection<Bebida> Bebidas
    {
        get => _bebidasService.Bebidas;
    }

    public decimal Total
    {
        get => _total;
        set
        {
            if (_total != value)
            {
                _total = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand AdicionarQuantidadeCommand { get; }
    public ICommand RemoverQuantidadeCommand { get; }
    public ICommand LimparPedidoCommand { get; }
    public ICommand FecharPedidoCommand { get; }
    public ICommand VerHistoricoCommand { get; }

    public PedidoViewModel(BebidasService bebidasService, HistoricoPedidosService historicoPedidosService)
    {
        _bebidasService = bebidasService;
        _historicoPedidosService = historicoPedidosService;
        AdicionarQuantidadeCommand = new Command<Bebida>(AdicionarQuantidade);
        RemoverQuantidadeCommand = new Command<Bebida>(RemoverQuantidade);
        LimparPedidoCommand = new Command(async () => await LimparPedidoAsync());
        FecharPedidoCommand = new Command(async () => await FecharPedidoAsync());
        VerHistoricoCommand = new Command(async () => await VerHistoricoAsync());
    }

    private void AdicionarQuantidade(Bebida bebida)
    {
        if (bebida != null)
        {
            bebida.Quantidade++;
            CalcularTotal();
        }
    }

    private void RemoverQuantidade(Bebida bebida)
    {
        if (bebida != null && bebida.Quantidade > 0)
        {
            bebida.Quantidade--;
            CalcularTotal();
        }
    }

    private async Task LimparPedidoAsync()
    {
        var confirmou = await Shell.Current.DisplayAlertAsync(
            "Confirmar",
            "Tens a certeza que queres limpar o pedido atual?",
            "Limpar",
            "Cancelar");

        if (confirmou)
        {
            LimparPedido();
        }
    }

    private void LimparPedido()
    {
        foreach (var bebida in Bebidas)
        {
            bebida.Quantidade = 0;
        }
        CalcularTotal();
    }

    private void CalcularTotal()
    {
        Total = Bebidas.Sum(b => b.Subtotal);
    }

    private async Task FecharPedidoAsync()
    {
        if (Total > 0)
        {
            // Create order history entry
            var pedidoHistorico = new PedidoHistorico
            {
                DataHora = DateTime.Now,
                Bebidas = Bebidas.Where(b => b.Quantidade > 0).Select(b => new Bebida
                {
                    Nome = b.Nome,
                    Preco = b.Preco,
                    Quantidade = b.Quantidade
                }).ToList(),
                Total = Total
            };

            await _historicoPedidosService.AddAsync(pedidoHistorico);

            // Clear current order
            LimparPedido();
        }
    }

    private async Task VerHistoricoAsync()
    {
        await Application.Current.MainPage.Navigation.PushAsync(new Pages.HistoricoPedidosPage());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}