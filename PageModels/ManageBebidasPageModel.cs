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
        private string _novaBebidaCategoria = CategoriasBebidas.Todas[0];
        private string _erroNome = string.Empty;
        private string _erroPreco = string.Empty;
        private Bebida? _bebidaEmEdicao;

        public ObservableCollection<Bebida> Bebidas
        {
            get => _bebidasService.Bebidas;
        }

        public IReadOnlyList<string> CategoriasDisponiveis => CategoriasBebidas.Todas;

        public string NovaBebidaCategoria
        {
            get => _novaBebidaCategoria;
            set { if (_novaBebidaCategoria != value) { _novaBebidaCategoria = value; OnPropertyChanged(); } }
        }

        public string ErroNome { get => _erroNome; private set { if (_erroNome != value) { _erroNome = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErroNome)); } } }
        public string ErroPreco { get => _erroPreco; private set { if (_erroPreco != value) { _erroPreco = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErroPreco)); } } }
        public bool TemErroNome => !string.IsNullOrEmpty(ErroNome);
        public bool TemErroPreco => !string.IsNullOrEmpty(ErroPreco);

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
            NovaBebidaCategoria = CategoriasBebidas.Todas.Contains(bebida.Categoria)
                ? bebida.Categoria
                : "Outros";
            OnPropertyChanged(nameof(EstaAEditar));
            OnPropertyChanged(nameof(TextoFormulario));
        }

        private async Task GuardarBebidaAsync()
        {
            ErroNome = string.Empty;
            ErroPreco = string.Empty;
            var nome = NovaBebidaNome.Trim();
            var bebidaComMesmoNome = Bebidas.FirstOrDefault(b =>
                !ReferenceEquals(b, _bebidaEmEdicao) &&
                string.Equals(b.Nome.Trim(), nome, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(nome))
            {
                ErroNome = "Introduza o nome da bebida.";
            }
            else if (bebidaComMesmoNome is not null)
            {
                ErroNome = "Já existe uma bebida com este nome.";
            }

            if (!decimal.TryParse(NovaBebidaPreco, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-PT"), out var preco) || preco <= 0)
            {
                ErroPreco = "Introduza um preço superior a zero.";
            }

            if (!string.IsNullOrEmpty(ErroNome) || !string.IsNullOrEmpty(ErroPreco)) return;

            if (_bebidaEmEdicao is null)
            {
                await _bebidasService.AddAsync(new Bebida
                {
                    Nome = NovaBebidaNome.Trim(),
                    Preco = preco,
                    Categoria = NovaBebidaCategoria.Trim()
                });
            }
            else
            {
                _bebidaEmEdicao.Nome = NovaBebidaNome.Trim();
                _bebidaEmEdicao.Preco = preco;
                _bebidaEmEdicao.Categoria = NovaBebidaCategoria.Trim();
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

            if (bebida.Quantidade > 0)
            {
                var removeDoPedido = await Shell.Current.DisplayAlertAsync(
                    "Bebida no pedido atual",
                    $"Esta bebida está no pedido atual com quantidade {bebida.Quantidade}. Eliminar mesmo assim?",
                    "Eliminar",
                    "Cancelar");
                if (!removeDoPedido) return;
                bebida.Quantidade = 0;
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
            NovaBebidaCategoria = CategoriasBebidas.Todas[0];
            ErroNome = string.Empty;
            ErroPreco = string.Empty;
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

        public async Task MostrarMigracoesCategoriasAsync()
        {
            var migracoes = await _bebidasService.ConsumirMigracoesCategoriasAsync();
            if (migracoes.Count == 0)
            {
                return;
            }

            var detalhes = string.Join(Environment.NewLine, migracoes.Select(migracao =>
                $"{migracao.NomeBebida}: '{migracao.CategoriaAnterior}' → '{migracao.CategoriaNova}'"));
            await Shell.Current.DisplayAlertAsync(
                "Categorias normalizadas",
                $"As seguintes bebidas foram reatribuídas:{Environment.NewLine}{detalhes}",
                "Confirmar");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}