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
        // ==========================================
        // 1. CAMPOS PRIVADOS
        // ==========================================

        // Cliente HTTP utilizado para realizar as requisições para a API RAWG.
        private readonly HttpClient httpClient;

        // Chave da API RAWG carregada por variável de ambiente.
        // Isso evita expor a chave diretamente no código e no GitHub.
        private readonly string _apiKey =
            Environment.GetEnvironmentVariable("RAWG_API_KEY") ?? string.Empty;

        /// <summary>
        /// Inicializa uma nova instância do serviço da API RAWG.
        /// Configura o protocolo de segurança TLS 1.2 e adiciona o cabeçalho User-Agent exigido pela API.
        /// </summary>
        public RawgApiService()
        {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12;

            httpClient = new HttpClient();

            // Define um tempo máximo para a API responder.
            // Isso evita que o programa fique travado caso a API demore ou não responda.
            httpClient.Timeout = TimeSpan.FromSeconds(15);

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
                // ==========================================
                // 2. VALIDAÇÕES INICIAIS
                // ==========================================

                // Verifica se o usuário digitou algum termo para pesquisar.
                if (string.IsNullOrWhiteSpace(termoBusca))
                {
                    throw new Exception("O termo de busca não pode estar vazio.");
                }

                // Verifica se a chave da API foi configurada corretamente.
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    throw new Exception("Chave da API RAWG não configurada. Configure a variável de ambiente RAWG_API_KEY.");
                }

                // ==========================================
                // 3. MONTAGEM DA URL E REQUISIÇÃO
                // ==========================================

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

                // ==========================================
                // 4. CONTROLE DE ERROS DA RESPOSTA HTTP
                // ==========================================

                // Verifica se a API retornou algum erro.
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Chave da API RAWG inválida ou não autorizada.");
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new Exception("Acesso negado pela API RAWG. Verifique a chave utilizada.");
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("Endpoint da API RAWG não encontrado.");
                    }

                    if ((int)response.StatusCode == 429)
                    {
                        throw new Exception("Limite de requisições da API RAWG atingido. Tente novamente mais tarde.");
                    }

                    throw new Exception(
                        $"Erro na API RAWG. Status: {(int)response.StatusCode} - {response.ReasonPhrase}"
                    );
                }

                // ==========================================
                // 5. LEITURA E VALIDAÇÃO DO JSON
                // ==========================================

                // Converte a resposta JSON em um documento manipulável.
                using JsonDocument doc =
                    JsonDocument.Parse(jsonResponse);

                JsonElement root = doc.RootElement;

                // Obtém a propriedade "results", onde ficam os jogos retornados pela API.
                // Caso ela não exista, significa que a resposta veio em um formato inesperado.
                if (!root.TryGetProperty("results", out JsonElement results))
                {
                    throw new Exception("A resposta da API RAWG não contém a lista de resultados.");
                }

                List<Games> jogosEncontrados =
                    new List<Games>();

                // ==========================================
                // 6. CONVERSÃO DOS RESULTADOS PARA A MODEL
                // ==========================================

                // Percorre os jogos retornados e converte cada item para o modelo Games.
                foreach (JsonElement jogo in results.EnumerateArray())
                {
                    // Evita erro caso algum item venha sem ID.
                    if (!jogo.TryGetProperty("id", out JsonElement idElement))
                    {
                        continue;
                    }

                    jogosEncontrados.Add(new Games
                    {
                        Id = idElement.GetInt32(),

                        Nome =
                            jogo.TryGetProperty("name",
                            out JsonElement nome) &&
                            nome.ValueKind != JsonValueKind.Null
                            ? nome.GetString()
                            : "",

                        ImagemUrl =
                            jogo.TryGetProperty("background_image", out JsonElement img) &&
                            img.ValueKind != JsonValueKind.Null
                            ? img.GetString() ?? string.Empty
                            : string.Empty,

                        Avaliacao =
                            jogo.TryGetProperty("rating",
                            out JsonElement rating) &&
                            rating.ValueKind != JsonValueKind.Null
                            ? rating.ToString()
                            : "0",

                        Classificacao =
                            jogo.TryGetProperty("metacritic",
                            out JsonElement meta) &&
                            meta.ValueKind != JsonValueKind.Null
                            ? meta.ToString()
                            : "0",

                        Upload = DateTime.Now
                    });
                }

                return jogosEncontrados;
            }
            catch (HttpRequestException)
            {
                // Erro de conexão, internet, DNS ou falha na comunicação HTTP.
                throw new Exception("Erro de conexão ao acessar a API RAWG. Verifique sua internet.");
            }
            catch (TaskCanceledException)
            {
                // Erro de tempo limite da requisição.
                throw new Exception("Tempo limite excedido ao consultar a API RAWG. Tente novamente.");
            }
            catch (JsonException)
            {
                // Erro ao interpretar o JSON retornado pela API.
                throw new Exception("Erro ao processar os dados retornados pela API RAWG.");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erro ao buscar na RAWG: {ex.Message}");
            }
        }
    }
}