using RaizBarApp.Models;
using System.Collections.ObjectModel;

namespace RaizBarApp.Services;

public sealed class BebidasService
{
    private readonly BebidaRepository _bebidaRepository;
    private readonly Task _initializationTask;

    public ObservableCollection<Bebida> Bebidas { get; } = new();

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
        await SaveAsync();
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
        Bebidas.Remove(bebida);
        await SaveAsync();
    }

    private async Task LoadBebidasAsync()
    {
        var bebidas = await _bebidaRepository.LoadBebidasAsync();
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var bebida in bebidas)
            {
                Bebidas.Add(bebida);
            }
        });
    }

    private Task SaveAsync()
    {
        return _bebidaRepository.SaveBebidasAsync(Bebidas.ToList());
    }
}
