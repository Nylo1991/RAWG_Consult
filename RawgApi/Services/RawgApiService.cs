using RawgApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RawgApi.Services
{
    /// <summary>
    /// Serviço responsável por realizar consultas de jogos na API RAWG.
    /// </summary>
    public class RawgApiService
    {
        private readonly HttpClient httpClient;

        private readonly string _apiKey =
            "53dad1fcdc4540d2a983ce2451f45b68";

        /// <summary>
        /// Inicializa uma nova instância do serviço da API RAWG.
        /// Configura o protocolo de segurança TLS 1.2 e adiciona o cabeçalho User-Agent exigido pela API.
        /// </summary>
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

        /// <summary>
        /// Busca jogos na API RAWG com base no termo informado pelo usuário.
        /// </summary>
        /// <param name="termoBusca">
        /// Termo digitado pelo usuário para pesquisar jogos.
        /// </param>
        /// <returns>
        /// Retorna uma lista de objetos Games contendo os jogos encontrados na API RAWG.
        /// </returns>
        /// <exception cref="Exception">
        /// Lança uma exceção caso ocorra erro durante a requisição, leitura ou conversão dos dados.
        /// </exception>
        public async Task<List<Games>> BuscarJogosAsync(string termoBusca)
        {
            try
            {
                // Monta a URL da requisição utilizando a chave da API e o termo de busca.
                string url =
                    $"https://api.rawg.io/api/games?key={_apiKey}" +
                    $"&search={Uri.EscapeDataString(termoBusca)}";

                // Envia a requisição GET para a API RAWG.
                HttpResponseMessage response =
                    await httpClient.GetAsync(url);

                // Lê o conteúdo retornado pela API em formato JSON.
                string jsonResponse =
                    await response.Content.ReadAsStringAsync();

                // DEBUG
                Console.WriteLine(jsonResponse);

                // Garante que a resposta da API foi bem-sucedida.
                response.EnsureSuccessStatusCode();

                // Converte a resposta JSON em um documento manipulável.
                using JsonDocument doc =
                    JsonDocument.Parse(jsonResponse);

                JsonElement root = doc.RootElement;

                // Obtém a propriedade "results", onde ficam os jogos retornados pela API.
                JsonElement results =
                    root.GetProperty("results");

                List<Games> jogosEncontrados =
                    new List<Games>();

                // Percorre os jogos retornados e converte cada item para o modelo Games.
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