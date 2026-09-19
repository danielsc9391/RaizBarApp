using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels;

public class PedidoViewModel : INotifyPropertyChanged
{
    private readonly BebidasService _bebidasService;
    private readonly HistoricoPedidosService _historicoPedidosService;
    private decimal _total;
    private bool _isEditing = false;
    private PedidoHistorico? _pedidoEditando = null;

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

    public string TotalFormatado => Total.ToString("C", new CultureInfo("pt-PT"));

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

    public void CarregarPedidoParaEdicao(PedidoHistorico pedido)
    {
        // Marcar que estamos em modo de edição
        _isEditing = true;
        _pedidoEditando = pedido;

        // Limpar o pedido atual
        LimparPedido();

        // Carregar as quantidades do pedido histórico
        foreach (var bebidaHistorica in pedido.Bebidas)
        {
            var bebidaAtual = Bebidas.FirstOrDefault(b => b.Nome == bebidaHistorica.Nome);
            if (bebidaAtual != null)
            {
                // Se a bebida existir no catálogo atual, preencher a quantidade
                bebidaAtual.Quantidade = bebidaHistorica.Quantidade;
            }
            // Se a bebida não existir no catálogo atual, ignoramos (comportamento razoável)
        }
        CalcularTotal();
    }

    private async Task FecharPedidoAsync()
    {
        if (Total > 0)
        {
            if (_isEditing && _pedidoEditando != null)
            {
                // Atualizar o pedido existente no histórico
                var pedidoParaAtualizar = _pedidoEditando;
                pedidoParaAtualizar.Bebidas.Clear();
                foreach (var bebida in Bebidas.Where(b => b.Quantidade > 0))
                {
                    pedidoParaAtualizar.Bebidas.Add(new Bebida
                    {
                        Nome = bebida.Nome,
                        Preco = bebida.Preco,
                        Quantidade = bebida.Quantidade
                    });
                }
                pedidoParaAtualizar.Total = Total;
                // Atualiza a data para a hora atual (ou mantém a original se preferir)
                // Aqui estou usando a data atual como padrão, mas poderia ser opcional
                pedidoParaAtualizar.DataHora = DateTime.Now;

                await _historicoPedidosService.SaveAsync(); // Este método já existe e salva todos os pedidos
            }
            else
            {
                // Criar novo pedido no histórico (comportamento original)
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
            }

            // Clear current order
            LimparPedido();
        }
    }

    private async Task VerHistoricoAsync()
    {
        await Shell.Current.Navigation.PushAsync(new Pages.HistoricoPedidosPage());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}