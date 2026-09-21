namespace RaizBarApp.Models;

public sealed class EstatisticaCategoria
{
    public string Categoria { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal Faturacao { get; init; }
    public double Percentagem { get; init; }
    public double LarguraBarra { get; init; }
    public string FaturacaoFormatada => $"{Faturacao:C}";
    public string PercentagemFormatada => $"{Percentagem:0.0}%";
}

public sealed class EstatisticaBebida
{
    public string Nome { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal Faturacao { get; init; }
    public string FaturacaoFormatada => $"{Faturacao:C}";
}

public sealed class EstatisticaHora
{
    public int Hora { get; init; }
    public int Pedidos { get; init; }
    public double AlturaBarra { get; init; }
    public string HoraFormatada => $"{Hora:00}h";
}
