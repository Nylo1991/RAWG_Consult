using RawgApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RawgApi.Services
{
    public class RawgApiService
    {
        private readonly HttpClient httpClient;

        private readonly string _apiKey =
            "53dad1fcdc4540d2a983ce2451f45b68";

        public RawgApiService()
        {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12;

            httpClient = new HttpClient();

            // RAWG exige User-Agent
            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "RawgWpfApp");
        }

        public async Task<List<Games>> BuscarJogosAsync(string termoBusca)
        {
            try
            {
                string url =
                    $"https://api.rawg.io/api/games?key={_apiKey}" +
                    $"&search={Uri.EscapeDataString(termoBusca)}";

                HttpResponseMessage response =
                    await httpClient.GetAsync(url);

                string jsonResponse =
                    await response.Content.ReadAsStringAsync();

                // DEBUG
                Console.WriteLine(jsonResponse);

                response.EnsureSuccessStatusCode();

                using JsonDocument doc =
                    JsonDocument.Parse(jsonResponse);

                JsonElement root = doc.RootElement;

                JsonElement results =
                    root.GetProperty("results");

                List<Games> jogosEncontrados =
                    new List<Games>();

                foreach (JsonElement jogo in results.EnumerateArray())
                {
                    jogosEncontrados.Add(new Games
                    {
                        Id = jogo.GetProperty("id").GetInt32(),

                        Nome =
                            jogo.TryGetProperty("name",
                            out JsonElement nome)
                            ? nome.GetString()
                            : "",

                        ImagemUrl =
    jogo.TryGetProperty("background_image", out JsonElement img) &&
    img.ValueKind != JsonValueKind.Null
        ? img.GetString() ?? string.Empty
        : string.Empty,

                        Avaliacao =
                            jogo.TryGetProperty("rating",
                            out JsonElement rating)
                            ? rating.ToString()
                            : "0",

                        Classificacao =
                            jogo.TryGetProperty("metacritic",
                            out JsonElement meta)
                            ? meta.ToString()
                            : "0",

                        Upload = DateTime.Now
                    });
                }

                return jogosEncontrados;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erro ao buscar na RAWG: {ex.Message}");
            }
        }
    }
}