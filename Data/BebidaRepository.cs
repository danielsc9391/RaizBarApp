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

                var bebidas = System.Text.Json.JsonSerializer.Deserialize(json, JsonContext.Default.ListBebida);

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
                var json = System.Text.Json.JsonSerializer.Serialize(_bebidas, JsonContext.Default.ListBebida);
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