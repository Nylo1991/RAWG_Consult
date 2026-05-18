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
    public class Aluno2ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly string _aluno2Url = "https://api-rawg.runasp.net/api/Jogos";

        public Aluno2ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> EnviarJogoAsync(Games jogo)
        {
            try
            {
                var jogoParaApi = new
                {
                    Id = jogo.Id.ToString(),

                    Nome = jogo.Nome ?? string.Empty,

                    Descricao = jogo.Descricao ?? string.Empty,

                    ImagemUrl = jogo.ImagemUrl ?? string.Empty,

                    Avaliacao = jogo.Avaliacao ?? "0",

                    // A API espera INT, não string vazia
                    Classificacao = ConverterClassificacao(jogo.Classificacao),

                    // A API exige DateTime em UTC
                    Upload = ConverterParaUtc(jogo.Upload)
                };

                string json = JsonSerializer.Serialize(jogoParaApi);

                Debug.WriteLine("JSON enviado para API:");
                Debug.WriteLine(json);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await _httpClient.PostAsync(_aluno2Url, content);

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

            // Quando vem do SQLite, normalmente vem como Unspecified.
            // Então tratamos como horário local e convertemos para UTC.
            return DateTime.SpecifyKind(data, DateTimeKind.Local).ToUniversalTime();
        }
    }
}