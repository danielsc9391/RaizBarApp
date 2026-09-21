using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace RaizBarApp.Services;

public sealed class BebidasService
{
    private readonly BebidaRepository _bebidaRepository;
    private readonly Task _initializationTask;

    public ObservableCollection<Bebida> Bebidas { get; } = new();
    private IReadOnlyList<MigracaoCategoriaBebida> _migracoesCategorias = Array.Empty<MigracaoCategoriaBebida>();

    public Task EnsureLoadedAsync() => _initializationTask;

    public BebidasService(BebidaRepository bebidaRepository)
    {
        _bebidaRepository = bebidaRepository;
        _initializationTask = LoadBebidasAsync();
    }

    public async Task AddAsync(Bebida bebida)
    {
        ArgumentNullException.ThrowIfNull(bebida);
        await _initializationTask;
        Bebidas.Add(bebida);
        try
        {
            await SaveAsync();
        }
        catch
        {
            Bebidas.Remove(bebida);
            throw;
        }
    }

    public async Task UpdateAsync(Bebida bebida)
    {
        ArgumentNullException.ThrowIfNull(bebida);
        await _initializationTask;
        await SaveAsync();
    }

    public async Task RemoveAsync(Bebida bebida)
    {
        ArgumentNullException.ThrowIfNull(bebida);
        await _initializationTask;
        var index = Bebidas.IndexOf(bebida);
        Bebidas.Remove(bebida);
        try
        {
            await SaveAsync();
        }
        catch
        {
            Bebidas.Insert(index < 0 ? Bebidas.Count : index, bebida);
            throw;
        }
    }

    public async Task<IReadOnlyList<MigracaoCategoriaBebida>> ConsumirMigracoesCategoriasAsync()
    {
        await _initializationTask;
        var migracoes = _migracoesCategorias;
        _migracoesCategorias = Array.Empty<MigracaoCategoriaBebida>();
        return migracoes;
    }

    private async Task LoadBebidasAsync()
    {
        var bebidas = await _bebidaRepository.LoadBebidasAsync();
        var migracoes = NormalizarCategorias(bebidas);
        if (migracoes.Count > 0)
        {
            await _bebidaRepository.SaveBebidasAsync(bebidas);
        }

        _migracoesCategorias = migracoes;
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var bebida in bebidas)
            {
                Bebidas.Add(bebida);
            }
        });
    }

    private static List<MigracaoCategoriaBebida> NormalizarCategorias(List<Bebida> bebidas)
    {
        var migracoes = new List<MigracaoCategoriaBebida>();

        foreach (var bebida in bebidas)
        {
            var categoriaAnterior = bebida.Categoria?.Trim() ?? string.Empty;
            var categoriaNova = EncontrarCategoria(categoriaAnterior);
            if (categoriaAnterior == categoriaNova)
            {
                continue;
            }

            bebida.Categoria = categoriaNova;
            migracoes.Add(new MigracaoCategoriaBebida(bebida.Nome, categoriaAnterior, categoriaNova));
        }

        return migracoes;
    }

    private static string EncontrarCategoria(string categoria)
    {
        var normalizada = NormalizarTexto(categoria);
        return normalizada switch
        {
            "agua" => "Água",
            "refrigerante" => "Refrigerante",
            "cerveja" or "vinho" or "destilados" => "Álcool",
            "alcool" => "Álcool",
            "outros" => "Outros",
            _ => "Outros"
        };
    }

    private static string NormalizarTexto(string texto)
    {
        var decomposed = texto.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

    private Task SaveAsync()
    {
        return _bebidaRepository.SaveBebidasAsync(Bebidas.ToList());
    }
}
