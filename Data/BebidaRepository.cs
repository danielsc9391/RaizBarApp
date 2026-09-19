using System.Text.Json;
using RaizBarApp.Models;

namespace RaizBarApp.Data
{
    public class BebidaRepository
    {
        private readonly string _filePath;
        private List<Bebida> _bebidas = new List<Bebida>();

        public BebidaRepository()
        {
            // Use AppDataDirectory for storing the beverages JSON file
            var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appDataDirectory, "RaizBarApp", "bebidas.json");
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        }

        public async Task<List<Bebida>> LoadBebidasAsync()
        {
            if (!File.Exists(_filePath))
            {
                // Return empty list if file doesn't exist yet (first run)
                _bebidas = new List<Bebida>();
                return _bebidas;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _bebidas = new List<Bebida>();
                    return _bebidas;
                }

                var bebidas = JsonSerializer.Deserialize<List<Bebida>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _bebidas = bebidas ?? new List<Bebida>();
                return _bebidas;
            }
            catch (Exception ex)
            {
                // In case of error, return empty list
                _bebidas = new List<Bebida>();
                return _bebidas;
            }
        }

        public async Task SaveBebidasAsync(List<Bebida> bebidas)
        {
            try
            {
                _bebidas = bebidas ?? new List<Bebida>();
                var json = JsonSerializer.Serialize(_bebidas, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                // Handle error appropriately in a real app
                throw;
            }
        }

        public async Task AddBebidaAsync(Bebida bebida)
        {
            _bebidas.Add(bebida);
            await SaveBebidasAsync(_bebidas);
        }

        public async Task UpdateBebidaAsync(Bebida bebida)
        {
            var index = _bebidas.FindIndex(b => b.Nome == bebida.Nome);
            if (index >= 0)
            {
                _bebidas[index] = bebida;
                await SaveBebidasAsync(_bebidas);
            }
        }

        public async Task RemoveBebidaAsync(Bebida bebida)
        {
            _bebidas.Remove(bebida);
            await SaveBebidasAsync(_bebidas);
        }

        public List<Bebida> GetBebidas()
        {
            return _bebidas;
        }
    }
}