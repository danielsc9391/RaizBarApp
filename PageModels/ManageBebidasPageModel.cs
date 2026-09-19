using RaizBarApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaizBarApp.PageModels
{
    public class ManageBebidasPageModel : INotifyPropertyChanged
    {
        private readonly BebidasService _bebidasService;
        private string _novaBebidaNome = string.Empty;
        private string _novaBebidaPreco = string.Empty;
        private Bebida? _bebidaEmEdicao;

        public ObservableCollection<Bebida> Bebidas
        {
            get => _bebidasService.Bebidas;
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

        public bool EstaAEditar => _bebidaEmEdicao is not null;

        public string TextoFormulario => EstaAEditar ? "Guardar alterações" : "Adicionar";

        public ICommand AddBebidaCommand { get; }
        public ICommand EditBebidaCommand { get; }
        public ICommand RemoveBebidaCommand { get; }
        public ICommand CancelarEdicaoCommand { get; }

        public ManageBebidasPageModel(BebidasService bebidasService)
        {
            _bebidasService = bebidasService;
            AddBebidaCommand = new Command(async () => await GuardarBebidaAsync());
            EditBebidaCommand = new Command<Bebida>(EditBebida);
            RemoveBebidaCommand = new Command<Bebida>(async bebida => await RemoveBebidaAsync(bebida));
            CancelarEdicaoCommand = new Command(CancelarEdicao);
        }

        private void EditBebida(Bebida? bebida)
        {
            if (bebida is null)
            {
                return;
            }

            _bebidaEmEdicao = bebida;
            NovaBebidaNome = bebida.Nome;
            NovaBebidaPreco = bebida.Preco.ToString("0.00", CultureInfo.GetCultureInfo("pt-PT"));
            OnPropertyChanged(nameof(EstaAEditar));
            OnPropertyChanged(nameof(TextoFormulario));
        }

        private async Task GuardarBebidaAsync()
        {
            if (string.IsNullOrWhiteSpace(NovaBebidaNome))
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Por favor, introduza o nome da bebida.", "OK");
                return;
            }

            if (!decimal.TryParse(NovaBebidaPreco, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-PT"), out var preco) || preco < 0)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Por favor, introduza um preço válido.", "OK");
                return;
            }

            if (_bebidaEmEdicao is null)
            {
                await _bebidasService.AddAsync(new Bebida
                {
                    Nome = NovaBebidaNome.Trim(),
                    Preco = preco
                });
            }
            else
            {
                _bebidaEmEdicao.Nome = NovaBebidaNome.Trim();
                _bebidaEmEdicao.Preco = preco;
                await _bebidasService.UpdateAsync(_bebidaEmEdicao);
            }

            CancelarEdicao();
        }

        private async Task RemoveBebidaAsync(Bebida? bebida)
        {
            if (bebida is null)
            {
                return;
            }

            var confirmou = await Shell.Current.DisplayAlertAsync(
                "Eliminar bebida?",
                $"Tens a certeza que queres eliminar '{bebida.Nome}'? Esta ação não pode ser desfeita.",
                "Eliminar",
                "Cancelar");

            if (confirmou)
            {
                await _bebidasService.RemoveAsync(bebida);
            }
        }

        private void CancelarEdicao()
        {
            _bebidaEmEdicao = null;
            NovaBebidaNome = string.Empty;
            NovaBebidaPreco = string.Empty;
            OnPropertyChanged(nameof(EstaAEditar));
            OnPropertyChanged(nameof(TextoFormulario));
        }

        public Task AdicionarBebidaAsync(Bebida bebida)
        {
            return _bebidasService.AddAsync(bebida);
        }

        public Task AtualizarBebidaAsync(Bebida bebida)
        {
            return _bebidasService.UpdateAsync(bebida);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}