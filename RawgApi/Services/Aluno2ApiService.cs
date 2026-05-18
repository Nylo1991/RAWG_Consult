using RawgApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RawgApi.Services
{
    /// <summary>
    /// Serviço responsável pelo envio dos jogos salvos para a API externa.
    /// </summary>
    public class Aluno2ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly string _aluno2Url = "https://api-rawg.runasp.net/api/Jogos";

        /// <summary>
        /// Inicializa o serviço e cria o cliente HTTP utilizado nas requisições.
        /// </summary>
        public Aluno2ApiService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Envia um jogo para a API externa.
        /// </summary>
        /// <param name="jogo">Jogo que será enviado para a API.</param>
        /// <returns>
        /// Retorna true se o envio for realizado com sucesso.
        /// Retorna false se a API retornar erro ou ocorrer alguma falha na requisição.
        /// </returns>
        public async Task<bool> EnviarJogoAsync(Games jogo)
        {
            try
            {
                // Cria um objeto no formato esperado pela API externa.
                var jogoParaApi = new
                {
                    Id = jogo.Id.ToString(),
                    Nome = jogo.Nome ?? string.Empty,
                    Descricao = jogo.Descricao ?? string.Empty,
                    ImagemUrl = jogo.ImagemUrl ?? string.Empty,
                    Avaliacao = jogo.Avaliacao ?? "0",

                    // A API espera a classificação como número inteiro.
                    Classificacao = ConverterClassificacao(jogo.Classificacao),

                    // A API exige que a data seja enviada em UTC.
                    Upload = ConverterParaUtc(jogo.Upload)
                };

                // Converte o objeto para JSON.
                string json = JsonSerializer.Serialize(jogoParaApi);

                Debug.WriteLine("JSON enviado para API:");
                Debug.WriteLine(json);

                // Prepara o conteúdo da requisição HTTP.
                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                // Envia os dados para a API externa.
                HttpResponseMessage response = await _httpClient.PostAsync(_aluno2Url, content);

                // Lê a resposta retornada pela API.
                string respostaApi = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Status API: {(int)response.StatusCode} - {response.ReasonPhrase}");
                Debug.WriteLine("Resposta API:");
                Debug.WriteLine(respostaApi);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao enviar para API:");
                Debug.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Debug.WriteLine("InnerException:");
                    Debug.WriteLine(ex.InnerException.Message);
                }

                return false;
            }
        }

        /// <summary>
        /// Envia uma lista de jogos para a API externa.
        /// </summary>
        /// <param name="jogos">Lista de jogos que serão enviados.</param>
        /// <returns>
        /// Retorna true se todos os jogos forem enviados com sucesso.
        /// Retorna false se pelo menos um jogo falhar no envio.
        /// </returns>
        /// <exception cref="Exception">
        /// Lançada quando ocorre erro inesperado durante o envio da lista.
        /// </exception>
        public async Task<bool> EnviarDadosAluno2Async(List<Games> jogos)
        {
            try
            {
                int sucessos = 0;
                int falhas = 0;

                foreach (var jogo in jogos)
                {
                    bool ok = await EnviarJogoAsync(jogo);

                    if (ok)
                    {
                        sucessos++;
                    }
                    else
                    {
                        falhas++;
                    }
                }

                return falhas == 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar dados para Aluno 2: {ex.Message}");
            }
        }

        /// <summary>
        /// Converte a classificação do jogo para inteiro.
        /// </summary>
        /// <param name="classificacao">Classificação recebida como texto.</param>
        /// <returns>
        /// Retorna a classificação convertida para inteiro.
        /// Caso o valor seja nulo, vazio ou inválido, retorna 0.
        /// </returns>
        private int ConverterClassificacao(string classificacao)
        {
            if (string.IsNullOrWhiteSpace(classificacao))
            {
                return 0;
            }

            if (int.TryParse(classificacao, out int valor))
            {
                return valor;
            }

            return 0;
        }

        /// <summary>
        /// Converte a data do jogo para UTC.
        /// </summary>
        /// <param name="data">Data original do jogo.</param>
        /// <returns>
        /// Retorna a data convertida para UTC.
        /// Caso a data esteja no valor padrão, retorna a data e hora atual em UTC.
        /// </returns>
        private DateTime ConverterParaUtc(DateTime data)
        {
            if (data == default)
            {
                return DateTime.UtcNow;
            }

            if (data.Kind == DateTimeKind.Utc)
            {
                return data;
            }

            if (data.Kind == DateTimeKind.Local)
            {
                return data.ToUniversalTime();
            }

            // Quando a data vem do SQLite, normalmente ela vem como Unspecified.
            // Nesse caso, tratamos como horário local e convertemos para UTC.
            return DateTime.SpecifyKind(data, DateTimeKind.Local).ToUniversalTime();
        }
    }
}