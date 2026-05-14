using RawgApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RawgApi.Services
{
    public class Aluno2ApiService
    {
        private readonly HttpClient _httpClient;
        // A URL base q o Aluno 2 vai me passar que é o MonsterAPI
        private readonly string _aluno2Url = "https://api.monsterapi.com.br/v1/aluno2";

        public Aluno2ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task EnviarDadosAluno2Async(List<Games> jogos)
        {
            try
            {
                string json = JsonSerializer.Serialize(jogos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(_aluno2Url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar dados para Aluno 2: {ex.Message}");
            }
        }
    }
}
