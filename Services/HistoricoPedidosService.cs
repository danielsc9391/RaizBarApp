using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace RaizBarApp.Services;

public sealed class HistoricoPedidosService
{
    private readonly string _filePath;
    private readonly Task _initializationTask;

    public ObservableCollection<PedidoHistorico> Pedidos { get; } = new();

    public HistoricoPedidosService()
    {
        try
        {
            var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Debug.WriteLine($"Diretório de dados do aplicativo: {appDataDirectory}");
            _filePath = Path.Combine(appDataDirectory, "RaizBarApp", "historico-pedidos.json");
            Debug.WriteLine($"Inicializando serviço de histórico com caminho: {_filePath}");
            // Verifying that the path is valid and accessible
            var directoryPath = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Debug.WriteLine($"Verificando diretório: {directoryPath}");
                Directory.CreateDirectory(directoryPath);
                Debug.WriteLine("Diretório criado ou já existente com sucesso");
            }
            _initializationTask = LoadFromFileAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro durante inicialização do serviço de histórico: {ex.Message}");
            Debug.WriteLine($"Detalhes do erro: {ex}");
            // Initialize with empty collection as fallback
            _filePath = Path.Combine(Path.GetTempPath(), "RaizBarApp", "historico-pedidos.json");
            _initializationTask = Task.CompletedTask;
        }
    }

    public async Task LoadAsync()
    {
        Debug.WriteLine("Iniciando carregamento do histórico de pedidos...");
        await _initializationTask;
        Debug.WriteLine("Carregamento do histórico de pedidos concluído");
        await LoadFromFileAsync();
    }

    public async Task AddAsync(PedidoHistorico pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        await _initializationTask;
        
        try
        {
            Debug.WriteLine($"Adicionando novo pedido ao histórico. Data: {pedido.DataHora}");
            Pedidos.Add(pedido);
            // Re-sort the collection to ensure newest orders appear first
            var sortedPedidos = Pedidos.OrderByDescending(p => p.DataHora).ToList();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    Pedidos.Clear();
                    foreach (var sortedPedido in sortedPedidos)
                    {
                        Pedidos.Add(sortedPedido);
                    }
                    Debug.WriteLine("Pedido adicionado e coleção reordenada com sucesso");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao atualizar coleção após adição de pedido: {ex.Message}");
                }
            });
            await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao adicionar pedido ao histórico: {ex.Message}");
        }
    }

    public async Task RemoveAsync(PedidoHistorico pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        await _initializationTask;

        try
        {
            Debug.WriteLine($"Removendo pedido do histórico. Data: {pedido.DataHora}");
            if (Pedidos.Remove(pedido))
            {
                // Re-sort the collection to ensure newest orders appear first
                var sortedPedidos = Pedidos.OrderByDescending(p => p.DataHora).ToList();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        Pedidos.Clear();
                        foreach (var sortedPedido in sortedPedidos)
                        {
                            Pedidos.Add(sortedPedido);
                        }
                        Debug.WriteLine("Pedido removido e coleção reordenada com sucesso");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erro ao atualizar coleção após remoção de pedido: {ex.Message}");
                    }
                });
                await SaveAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao remover pedido do histórico: {ex.Message}");
        }
    }

    private async Task LoadFromFileAsync()
    {
        try
        {
            Debug.WriteLine($"Tentando carregar histórico de pedidos do caminho: {_filePath}");

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (!File.Exists(_filePath))
            {
                Debug.WriteLine($"Histórico de pedidos não encontrado. Será criado em: {_filePath}");
                await ReplacePedidosAsync([]);
                return;
            }

            Debug.WriteLine("Ficheiro encontrado, lendo conteúdo...");
            var json = await File.ReadAllTextAsync(_filePath);
            Debug.WriteLine($"Conteúdo lido com sucesso. Tamanho: {json.Length} caracteres");

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine($"O ficheiro do histórico está vazio: {_filePath}");
                await ReplacePedidosAsync([]);
                return;
            }

            Debug.WriteLine("Deserializando JSON...");
            var pedidos = JsonSerializer.Deserialize(json, JsonContext.Default.ListPedidoHistorico);
            Debug.WriteLine($"Deserialização concluída. Encontrados {pedidos?.Count ?? 0} pedidos");

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
            Debug.WriteLine($"O ficheiro do histórico está corrompido ou tem JSON inválido ({_filePath}): {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
            await ReplacePedidosAsync([]);
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.WriteLine($"Acesso negado ao ficheiro do histórico ({_filePath}): {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
            await ReplacePedidosAsync([]);
        }
        catch (DirectoryNotFoundException exception)
        {
            Debug.WriteLine($"Diretório não encontrado para o histórico de pedidos ({_filePath}): {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
            await ReplacePedidosAsync([]);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Erro ao carregar o histórico de pedidos de {_filePath}: {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
            await ReplacePedidosAsync([]);
        }
    }

    private Task ReplacePedidosAsync(IEnumerable<PedidoHistorico> pedidos)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                Debug.WriteLine($"Atualizando ObservableCollection com {pedidos.Count()} pedidos");
                Pedidos.Clear();
                foreach (var pedido in pedidos.OrderByDescending(p => p.DataHora))
                {
                    Pedidos.Add(pedido);
                }
                Debug.WriteLine("Atualização da ObservableCollection concluída com sucesso");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao atualizar ObservableCollection: {ex.Message}");
                Debug.WriteLine($"Detalhes do erro: {ex}");
                // Clear the collection in case of error to prevent inconsistent state
                Pedidos.Clear();
            }
        });
    }

    public async Task SaveAsync()
    {
        try
        {
            Debug.WriteLine("Salvando histórico de pedidos...");
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonSerializer.Serialize(Pedidos.ToList(), JsonContext.Default.ListPedidoHistorico);
            var temporaryPath = _filePath + ".tmp";
            try
            {
                await using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                await using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                    await writer.FlushAsync();
                    await stream.FlushAsync();
                }

                File.Move(temporaryPath, _filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            Debug.WriteLine("Histórico de pedidos salvo com sucesso");
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.WriteLine($"Acesso negado ao salvar o histórico de pedidos ({_filePath}): {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
        }
        catch (DirectoryNotFoundException exception)
        {
            Debug.WriteLine($"Diretório não encontrado ao salvar o histórico de pedidos ({_filePath}): {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Erro ao salvar o histórico de pedidos em {_filePath}: {exception.Message}");
            Debug.WriteLine($"Detalhes do erro: {exception}");
        }
    }
}
