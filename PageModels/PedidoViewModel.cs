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
    private bool _isEditingExistingOrder;
    private PedidoHistorico? _pedidoEditando = null;

    public ObservableCollection<Bebida> Bebidas
    {
        get => _bebidasService.Bebidas;
    }

    public ObservableCollection<CategoriaBebidasGroup> BebidasAgrupadas { get; } = new();

    public decimal Total
    {
        get => _total;
        set
        {
            if (_total != value)
            {
                _total = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalFormatado));
            }
        }
    }

    public string TotalFormatado => Total.ToString("C", new CultureInfo("pt-PT"));

    public bool IsEditingExistingOrder
    {
        get => _isEditingExistingOrder;
        private set
        {
            if (_isEditingExistingOrder != value)
            {
                _isEditingExistingOrder = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand AdicionarQuantidadeCommand { get; }
    public ICommand RemoverQuantidadeCommand { get; }
    public ICommand LimparPedidoCommand { get; }
    public ICommand FecharPedidoCommand { get; }
    public ICommand VerHistoricoCommand { get; }
    public ICommand AbrirGestaoBebidasCommand { get; }

    public PedidoViewModel(BebidasService bebidasService, HistoricoPedidosService historicoPedidosService)
    {
        _bebidasService = bebidasService;
        _historicoPedidosService = historicoPedidosService;
        AdicionarQuantidadeCommand = new Command<object>(obj =>
        {
            if (obj is Bebida bebida)
                AdicionarQuantidade(bebida);
        });
        RemoverQuantidadeCommand = new Command<object>(obj =>
        {
            if (obj is Bebida bebida)
                RemoverQuantidade(bebida);
        });
        LimparPedidoCommand = new Command(async () => await LimparPedidoAsync());
        FecharPedidoCommand = new Command(async () => await FecharPedidoAsync());
        VerHistoricoCommand = new Command(async () => await VerHistoricoAsync());
        AbrirGestaoBebidasCommand = new Command(async () => await AbrirGestaoBebidasAsync());

        // Adicionar handler para observar mudanças nas bebidas
        Bebidas.CollectionChanged += (sender, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (Bebida bebida in e.NewItems)
                {
                    bebida.PropertyChanged += BebidaPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (Bebida bebida in e.OldItems)
                {
                    bebida.PropertyChanged -= BebidaPropertyChanged;
                }
            }
            AtualizarAgrupamento();
        };

        // Subscrever eventos de propriedades para bebidas já existentes
        foreach (var bebida in Bebidas)
        {
            bebida.PropertyChanged += BebidaPropertyChanged;
        }
        AtualizarAgrupamento();
    }

    private void BebidaPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Se a mudança for na quantidade ou preço, recalcular o total
        if (e.PropertyName == nameof(Bebida.Quantidade) || e.PropertyName == nameof(Bebida.Preco))
        {
            CalcularTotal();
        }
        else if (e.PropertyName == nameof(Bebida.Categoria))
        {
            AtualizarAgrupamento();
        }
    }

    private void AtualizarAgrupamento()
    {
        var grupos = Bebidas
            .GroupBy(b => CategoriasBebidas.Todas.FirstOrDefault(categoria =>
                string.Equals(categoria, b.Categoria?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "Outros")
            .OrderBy(g => g.Key)
            .Select(g => new CategoriaBebidasGroup(g.Key, g.OrderBy(b => b.Nome)))
            .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            BebidasAgrupadas.Clear();
            foreach (var grupo in grupos) BebidasAgrupadas.Add(grupo);
        });
    }

    private async Task AbrirGestaoBebidasAsync()
    {
        await Shell.Current.GoToAsync("//manage-bebidas");
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
        IsEditingExistingOrder = true;
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
            var confirmou = await Shell.Current.DisplayAlertAsync(
                "Fechar este pedido?",
                $"Confirmas o fecho deste pedido com o total de {TotalFormatado}?",
                "Fechar",
                "Cancelar");

            if (!confirmou)
            {
                return;
            }

            if (IsEditingExistingOrder && _pedidoEditando != null)
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

                await _historicoPedidosService.SaveAsync();
                LimparPedido();
                IsEditingExistingOrder = false;
                _pedidoEditando = null;
                await Shell.Current.GoToAsync("//historico-pedidos");
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

                LimparPedido();
            }
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