using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using RaizBarApp.Models;

namespace RaizBarApp.Services;

public sealed class HistoricoPedidosService
{
    private readonly string _filePath;
    private readonly Task _initializationTask;

    public ObservableCollection<PedidoHistorico> Pedidos { get; } = new();

    public HistoricoPedidosService()
    {
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _filePath = Path.Combine(appDataDirectory, "RaizBarApp", "historico-pedidos.json");
        _initializationTask = LoadFromFileAsync();
    }

    public async Task LoadAsync()
    {
        await _initializationTask;
        await LoadFromFileAsync();
    }

    public async Task AddAsync(PedidoHistorico pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        await _initializationTask;
        Pedidos.Add(pedido);
        await SaveAsync();
    }

    public async Task RemoveAsync(PedidoHistorico pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        await _initializationTask;

        if (Pedidos.Remove(pedido))
        {
            await SaveAsync();
        }
    }

    private async Task LoadFromFileAsync()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (!File.Exists(_filePath))
            {
                Debug.WriteLine($"Histórico de pedidos não encontrado. Será criado em: {_filePath}");
                await ReplacePedidosAsync([]);
                return;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine($"O ficheiro do histórico está vazio: {_filePath}");
                await ReplacePedidosAsync([]);
                return;
            }

            var pedidos = JsonSerializer.Deserialize(json, JsonContext.Default.ListPedidoHistorico);
            if (pedidos is null)
            {
                Debug.WriteLine($"O ficheiro do histórico não contém uma lista válida: {_filePath}");
                await ReplacePedidosAsync([]);
                return;
            }

            await ReplacePedidosAsync(pedidos);
        }
        catch (JsonException exception)
        {
            Debug.WriteLine($"O ficheiro do histórico está corrompido ou tem JSON inválido ({_filePath}): {exception}");
            await ReplacePedidosAsync([]);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Erro ao carregar o histórico de pedidos de {_filePath}: {exception}");
            await ReplacePedidosAsync([]);
        }
    }

    private Task ReplacePedidosAsync(IEnumerable<PedidoHistorico> pedidos)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            Pedidos.Clear();
            foreach (var pedido in pedidos)
            {
                Pedidos.Add(pedido);
            }
        });
    }

    private async Task SaveAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var json = JsonSerializer.Serialize(Pedidos.ToList(), JsonContext.Default.ListPedidoHistorico);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
