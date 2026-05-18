using Microsoft.EntityFrameworkCore;
using RawgApi.Data;
using RawgApi.Models;
using RawgApi.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RawgApi.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RawgApiService _rawgApiService;
        private readonly LocalDbContex _dbContext;

        private string _termPesquisa;
        public string TermPesquisa
        {
            get => _termPesquisa;
            set
            {
                _termPesquisa = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private Games _selectedGame;
        public Games SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Games> ListaGames { get; set; }

        public ICommand FetchFromRawgCommand { get; }
        public ICommand SaveToLocalDbCommand { get; }

        public MainViewModel()
        {
            _rawgApiService = new RawgApiService();
            _dbContext = new LocalDbContex();

            ListaGames = new ObservableCollection<Games>();

            InicializarBanco();

            FetchFromRawgCommand = new RelayCommand(async (o) => await ProcurarNaRawg());
            SaveToLocalDbCommand = new RelayCommand((o) => SalvarLocal());

            // Tela principal inicia limpa
            TermPesquisa = string.Empty;
            SelectedGame = null;
            StatusMessage = "Digite o nome de um jogo para pesquisar.";
        }

        private void InicializarBanco()
        {
            try
            {
                _dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao iniciar banco SQLite:\n\n" + ObterErroCompleto(ex),
                    "Erro SQLite",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task ProcurarNaRawg()
        {
            if (string.IsNullOrWhiteSpace(TermPesquisa))
            {
                StatusMessage = "Digite um termo para pesquisar.";
                return;
            }

            try
            {
                StatusMessage = "Buscando na RAWG...";

                var resultados = await _rawgApiService.BuscarJogosAsync(TermPesquisa);

                ListaGames.Clear();

                foreach (var jogo in resultados)
                {
                    jogo.IsSelected = false;
                    ListaGames.Add(jogo);
                }

                SelectedGame = null;

                StatusMessage = $"{resultados.Count} jogo(s) encontrado(s). Selecione um jogo para salvar.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao buscar: " + ObterErroCompleto(ex);
            }
        }

        private void SalvarLocal()
        {
            try
            {
                Games jogo = SelectedGame;

                if (jogo == null)
                {
                    jogo = ListaGames.FirstOrDefault(g => g.IsSelected);
                }

                if (jogo == null)
                {
                    StatusMessage = "Selecione uma linha ou marque um jogo para salvar.";
                    return;
                }

                if (jogo.Id <= 0)
                {
                    StatusMessage = "Jogo inválido para salvar.";
                    return;
                }

                NormalizarJogo(jogo);

                bool jaExiste = _dbContext.Games.Any(g => g.Id == jogo.Id);

                if (jaExiste)
                {
                    StatusMessage = $"O jogo '{jogo.Nome}' já está salvo no SQLite.";
                    return;
                }

                var novoJogo = new Games
                {
                    Id = jogo.Id,
                    Nome = jogo.Nome,
                    Descricao = jogo.Descricao,
                    ImagemUrl = jogo.ImagemUrl,
                    Avaliacao = jogo.Avaliacao,
                    Classificacao = jogo.Classificacao,
                    Upload = DateTime.Now
                };

                _dbContext.Games.Add(novoJogo);
                _dbContext.SaveChanges();

                StatusMessage = $"Jogo '{jogo.Nome}' salvo com sucesso no SQLite.";
            }
            catch (DbUpdateException ex)
            {
                StatusMessage = "Erro ao salvar no SQLite: " + ObterErroCompleto(ex);
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro inesperado ao salvar: " + ObterErroCompleto(ex);
            }
        }

        private void NormalizarJogo(Games jogo)
        {
            jogo.Nome = jogo.Nome ?? string.Empty;
            jogo.Descricao = jogo.Descricao ?? string.Empty;
            jogo.ImagemUrl = jogo.ImagemUrl ?? string.Empty;
            jogo.Avaliacao = string.IsNullOrWhiteSpace(jogo.Avaliacao) ? "0" : jogo.Avaliacao;
            jogo.Classificacao = string.IsNullOrWhiteSpace(jogo.Classificacao) ? "0" : jogo.Classificacao;

            if (jogo.Upload == default)
            {
                jogo.Upload = DateTime.Now;
            }
        }

        private string ObterErroCompleto(Exception ex)
        {
            var mensagem = new StringBuilder();

            while (ex != null)
            {
                mensagem.AppendLine(ex.Message);
                ex = ex.InnerException;
            }

            return mensagem.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}