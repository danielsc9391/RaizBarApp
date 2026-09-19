using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels;

public class PedidoViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Bebida> _bebidas = new ObservableCollection<Bebida>();
    private decimal _total;
    private string _novaBebidaNome;
    private string _novaBebidaPreco;
    private static List<PedidoHistorico> _historicoPedidos = new List<PedidoHistorico>();

    public ObservableCollection<Bebida> Bebidas
    {
        get => _bebidas;
        set
        {
            if (_bebidas != value)
            {
                _bebidas = value ?? new ObservableCollection<Bebida>();
                OnPropertyChanged();
                CalcularTotal();
            }
        }
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

    public string NovaBebidaNome
    {
        get => _novaBebidaNome;
        set
        {
            if (_novaBebidaNome != value)
            {
                _novaBebidaNome = value;
                OnPropertyChanged();
            }
        }
    }

    public string NovaBebidaPreco
    {
        get => _novaBebidaPreco;
        set
        {
            if (_novaBebidaPreco != value)
            {
                _novaBebidaPreco = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand AdicionarQuantidadeCommand { get; }
    public ICommand RemoverQuantidadeCommand { get; }
    public ICommand LimparPedidoCommand { get; }
    public ICommand AddBebidaCommand { get; }
    public ICommand FecharPedidoCommand { get; }
    public ICommand VerHistoricoCommand { get; }

    public PedidoViewModel()
    {
        AdicionarQuantidadeCommand = new Command<Bebida>(AdicionarQuantidade);
        RemoverQuantidadeCommand = new Command<Bebida>(RemoverQuantidade);
        LimparPedidoCommand = new Command(LimparPedido);
        AddBebidaCommand = new Command(async () => await AddBebidaAsync());
        FecharPedidoCommand = new Command(async () => await FecharPedidoAsync());
        VerHistoricoCommand = new Command(async () => await VerHistoricoAsync());
        
        // Adicionando algumas bebidas de exemplo
        Bebidas.Add(new Bebida { Nome = "Cerveja", Preco = 8.50m });
        Bebidas.Add(new Bebida { Nome = "Vinho", Preco = 25.00m });
        Bebidas.Add(new Bebida { Nome = "Refrigerante", Preco = 6.00m });
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

    private async Task AddBebidaAsync()
    {
        // Create a new page for adding beverage and show it as a modal
        var addBebidaPage = new Pages.AddBebidaPage(this);
        await Application.Current.MainPage.Navigation.PushModalAsync(addBebidaPage, true);
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

            // Add to history
            _historicoPedidos.Add(pedidoHistorico);

            // Clear current order
            LimparPedido();
        }
    }

    private async Task VerHistoricoAsync()
    {
        await Application.Current.MainPage.Navigation.PushAsync(new Pages.HistoricoPedidosPage());
    }

    public static List<PedidoHistorico> HistoricoPedidos => _historicoPedidos;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}