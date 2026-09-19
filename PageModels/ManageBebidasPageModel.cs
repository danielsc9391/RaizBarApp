using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels
{
    public class ManageBebidasPageModel : INotifyPropertyChanged
    {
        private ObservableCollection<Bebida> _bebidas = new ObservableCollection<Bebida>();
        private string _novaBebidaNome;
        private string _novaBebidaPreco;
        private readonly BebidaRepository _bebidaRepository;

        public ObservableCollection<Bebida> Bebidas
        {
            get => _bebidas;
            set
            {
                if (_bebidas != value)
                {
                    _bebidas = value ?? new ObservableCollection<Bebida>();
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

        public ICommand AddBebidaCommand { get; }
        public ICommand EditBebidaCommand { get; }
        public ICommand RemoveBebidaCommand { get; }

        public ManageBebidasPageModel()
        {
            _bebidaRepository = new BebidaRepository();
            AddBebidaCommand = new Command(async () => await AddBebidaAsync());
            EditBebidaCommand = new Command<Bebida>(EditBebida);
            RemoveBebidaCommand = new Command<Bebida>(RemoveBebida);

            // Load beverages from persistence
            LoadBebidasAsync().Wait();
        }

        private async Task LoadBebidasAsync()
        {
            var bebidas = await _bebidaRepository.LoadBebidasAsync();
            Bebidas.Clear();
            foreach (var bebida in bebidas)
            {
                Bebidas.Add(bebida);
            }
        }

        private async Task AddBebidaAsync()
        {
            // Create a new page for adding beverage and show it as a modal
            var addBebidaPage = new Pages.AddBebidaPage(this);
            await Application.Current.MainPage.Navigation.PushModalAsync(addBebidaPage, true);
        }

        private void EditBebida(Bebida bebida)
        {
            // For now, just navigate to the AddBebidaPage with pre-filled data for editing
            // In a real implementation, you might want to create a separate edit page or modal
            if (bebida != null)
            {
                var addBebidaPage = new Pages.AddBebidaPage(this, bebida);
                Application.Current.MainPage.Navigation.PushModalAsync(addBebidaPage, true);
            }
        }

        private async void RemoveBebida(Bebida bebida)
        {
            if (bebida != null)
            {
                Bebidas.Remove(bebida);
                await _bebidaRepository.RemoveBebidaAsync(bebida);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}