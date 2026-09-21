using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RaizBarApp.PageModels;

public sealed class EstatisticasPageModel : INotifyPropertyChanged
{
    private static readonly string[] Categorias = ["Água", "Refrigerante", "Álcool", "Outros"];
    private readonly HistoricoPedidosService _historicoPedidosService;
    private bool _temDados;
    private string _horaDePico = string.Empty;

    public ObservableCollection<EstatisticaCategoria> FaturacaoPorCategoria { get; } = [];
    public ObservableCollection<EstatisticaBebida> FaturacaoPorBebida { get; } = [];
    public ObservableCollection<EstatisticaHora> PedidosPorHora { get; } = [];

    public bool TemDados
    {
        get => _temDados;
        private set
        {
            if (_temDados != value)
            {
                _temDados = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SemDados));
            }
        }
    }

    public bool SemDados => !TemDados;
    public string MensagemSemDados => "Ainda não há dados suficientes para gerar estatísticas";
    public string HoraDePico
    {
        get => _horaDePico;
        private set
        {
            if (_horaDePico != value)
            {
                _horaDePico = value;
                OnPropertyChanged();
            }
        }
    }

    public int TotalQuantidade { get; private set; }
    public decimal TotalFaturacao { get; private set; }
    public string TotalFaturacaoFormatada => $"{TotalFaturacao.ToString("C", CultureInfo.CurrentCulture)}";

    public EstatisticasPageModel(HistoricoPedidosService historicoPedidosService)
    {
        _historicoPedidosService = historicoPedidosService;
    }

    public async Task CarregarAsync()
    {
        await _historicoPedidosService.LoadAsync();
        Calcular(_historicoPedidosService.Pedidos);
    }

    private void Calcular(IEnumerable<PedidoHistorico> pedidos)
    {
        var pedidosLista = pedidos.ToList();
        var linhas = pedidosLista.SelectMany(pedido => pedido.Bebidas ?? []).ToList();
        var faturacaoPorLinha = linhas.Select(bebida => new
        {
            Bebida = bebida,
            Categoria = NormalizarCategoria(bebida.Categoria),
            Faturacao = bebida.Preco * bebida.Quantidade
        }).ToList();

        TotalQuantidade = linhas.Sum(bebida => bebida.Quantidade);
        TotalFaturacao = faturacaoPorLinha.Sum(linha => linha.Faturacao);
        OnPropertyChanged(nameof(TotalQuantidade));
        OnPropertyChanged(nameof(TotalFaturacao));
        OnPropertyChanged(nameof(TotalFaturacaoFormatada));

        FaturacaoPorCategoria.Clear();
        var maxFaturacaoCategoria = Categorias
            .Select(categoria => faturacaoPorLinha.Where(linha => linha.Categoria == categoria).Sum(linha => linha.Faturacao))
            .DefaultIfEmpty()
            .Max();

        foreach (var categoria in Categorias
                     .Select((nome, indice) => new
                     {
                         Nome = nome,
                         Indice = indice,
                         Linhas = faturacaoPorLinha.Where(linha => linha.Categoria == nome).ToList()
                     })
                     .Select(item => new EstatisticaCategoria
                     {
                         Categoria = item.Nome,
                         Quantidade = item.Linhas.Sum(linha => linha.Bebida.Quantidade),
                         Faturacao = item.Linhas.Sum(linha => linha.Faturacao),
                         Percentagem = TotalFaturacao == 0 ? 0 : (double)(item.Linhas.Sum(linha => linha.Faturacao) / TotalFaturacao * 100),
                         LarguraBarra = maxFaturacaoCategoria == 0 ? 0 : (double)(item.Linhas.Sum(linha => linha.Faturacao) / maxFaturacaoCategoria * 100)
                     })
                     .OrderByDescending(item => item.Faturacao))
        {
            FaturacaoPorCategoria.Add(categoria);
        }

        FaturacaoPorBebida.Clear();
        foreach (var bebida in faturacaoPorLinha
                     .GroupBy(linha => new { linha.Bebida.Nome, linha.Categoria })
                     .Select(grupo => new EstatisticaBebida
                     {
                         Nome = grupo.Key.Nome,
                         Categoria = grupo.Key.Categoria,
                         Quantidade = grupo.Sum(linha => linha.Bebida.Quantidade),
                         Faturacao = grupo.Sum(linha => linha.Faturacao)
                     })
                     .OrderByDescending(item => item.Faturacao)
                     .ThenBy(item => item.Nome))
        {
            FaturacaoPorBebida.Add(bebida);
        }

        PedidosPorHora.Clear();
        var contagensPorHora = Enumerable.Range(0, 24)
            .Select(hora => new { Hora = hora, Pedidos = pedidosLista.Count(pedido => pedido.DataHora.Hour == hora) })
            .ToList();
        var maiorContagem = contagensPorHora.Max(item => item.Pedidos);
        foreach (var item in contagensPorHora)
        {
            PedidosPorHora.Add(new EstatisticaHora
            {
                Hora = item.Hora,
                Pedidos = item.Pedidos,
                AlturaBarra = maiorContagem == 0 ? 0 : 150d * item.Pedidos / maiorContagem
            });
        }

        var horasDePico = contagensPorHora
            .Where(item => item.Pedidos == maiorContagem && item.Pedidos > 0)
            .Select(item => $"{item.Hora:00}h")
            .ToList();
        HoraDePico = horasDePico.Count == 0
            ? string.Empty
            : $"Maior movimento às {string.Join(" e ", horasDePico)}, com {maiorContagem} {(maiorContagem == 1 ? "pedido" : "pedidos")}.";
        TemDados = pedidosLista.Count > 0;
    }

    private static string NormalizarCategoria(string? categoria)
    {
        var valor = categoria?.Trim() ?? string.Empty;
        return Categorias.FirstOrDefault(item => string.Equals(item, valor, StringComparison.OrdinalIgnoreCase)) ?? "Outros";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
