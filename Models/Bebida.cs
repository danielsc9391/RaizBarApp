using System.ComponentModel;
using System.Globalization;

namespace RaizBarApp.Models;

public class Bebida : INotifyPropertyChanged
{
    private string _nome = string.Empty;
    private decimal _preco;
    private int _quantidade;

    public string Nome
    {
        get => _nome;
        set
        {
            if (_nome != value)
            {
                _nome = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }

    public decimal Preco
    {
        get => _preco;
        set
        {
            if (_preco != value)
            {
                _preco = value;
                OnPropertyChanged();
            }
        }
    }

    public int Quantidade
    {
        get => _quantidade;
        set
        {
            if (_quantidade != value)
            {
                _quantidade = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Subtotal => Preco * Quantidade;

    public Bebida()
    {
        _nome = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}