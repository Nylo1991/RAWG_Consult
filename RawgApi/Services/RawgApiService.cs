using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RawgApi.Models;


namespace RawgApi.Services
{
     public class RawgApiService
    {
        private readonly HttpClient httpClient;
        // Onde ficara o nosso acesso da API Publica que RAWG
        private readonly string _apiKey = "meu API Key";

        public RawgApiService()
        {
            httpClient = new HttpClient();
        }

        public async Task<List<Games>> BuscarJogosAsync(string termoBusca)
        {
            try
            {
                // Exemplo de endpoint para obter jogos populares
                string url = $"https://api.rawg.io/api/games?key={_apiKey}&search={termoBusca}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // NOTA: A API da RAWG retorna os jogos dentro de uma propriedade chamada "results".
                // Para simplificar a sua SA, estamos criando um objeto dinâmico aqui para extrair apenas o básico.
                // Em um cenário real, você criaria uma classe RawgResponse.
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;
                JsonElement results = root.GetProperty("results");

                List<Games> jogosEncontrados = new List<Games>();

                foreach (JsonElement jogo  in results.EnumerateArray()) 
                    {
                    jogosEncontrados.Add(new Games
                    {
                        Id = jogo.GetProperty("id").ToString(),
                        Nome = jogo.GetProperty("Name").GetString(),
                        // A imagem e outros dados podem vir nulos dependendo do jogo, é bom tratar
                        ImagemUrl = jogo.TryGetProperty("background_image", out JsonElement img) ? img.GetString() : null,
                        Avaliacao = jogo.TryGetProperty("rating", out JsonElement rating) ? rating.ToString() : "0",
                        Upload = DateTime.Now
                    });
                }

                return jogosEncontrados;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar na RAWG: {ex.Message}");
            }
        }
    }
}
