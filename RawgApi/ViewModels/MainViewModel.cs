using RawgApi.Data;
using RawgApi.Models;
using RawgApi.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace RawgApi.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        private readonly RawgApiService _rawgApiService;
        private readonly Aluno2ApiService _aluno2ApiService;
        private readonly LocalDbContex _dbContext;

        //propriedades conectadas a tela (binding)
        private string _termPesquisa;
        public string TermPesquisa
        {
            get => _termPesquisa;
            set { _termPesquisa = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Games> ListaGames { get; set; }

        // onde ficam os comandos (botões)
        public ICommand FetchFromRawgCommand { get; }
        public ICommand SendToAluno2Command { get; }
        public ICommand SaveToLocalDbCommand { get; }

        public MainViewModel()
        {
            _rawgApiService = new RawgApiService();
            _aluno2ApiService = new Aluno2ApiService();
            _dbContext = new LocalDbContex();

            // Garante q o banco SQLite seja criado ao abrir o app
            _dbContext.Database.EnsureCreated();

            ListaGames = new ObservableCollection<Games>();

            FetchFromRawgCommand = new RelayCommand(async (o) => await ProcurarNaRawg());
            SendToAluno2Command = new RelayCommand(async (o) => await MandarParaAluno2());
            SaveToLocalDbCommand = new RelayCommand(async (o) => SalvarLocal());
        }

        private async Task ProcurarNaRawg()
        {
            if (string.IsNullOrWhiteSpace(TermPesquisa)) return;

            StatusMessage = "Buscando...";
            try
            {
                var resultados = await _rawgApiService.BuscarJogosAsync(TermPesquisa);
                ListaGames.Clear();
                foreach (var jogo in resultados)
                {
                    ListaGames.Add(jogo);
                }
                StatusMessage = $"{resultados.Count} jogos encontrados!";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro: " + ex.Message;
            }
        }

        private void SalvarLocal()
        {
            try
            {
                StatusMessage = "Salvando no SQLite...";
                foreach (var jogo in ListaGames)
                {
                    // Verifica se o jogo já existe no banco local para não duplicar
                    if (!_dbContext.Games.Any(g => g.Id == jogo.Id))
                    {
                        _dbContext.Games.Add(jogo);
                    }
                }
                _dbContext.SaveChanges();
                StatusMessage = "Salvo no banco de dados local com sucesso!";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao salvar: " + ex.Message;
            }
        }

        private async Task MandarParaAluno2()
        {
            try
            {
                StatusMessage = "Enviando para a API do grupo...";
                // Pega todos os jogos do banco local para mandar pro Aluno 2
                var jogosParaEnviar = _dbContext.Games.ToList();

                await _aluno2ApiService.EnviarDadosAluno2Async(jogosParaEnviar);
                StatusMessage = "Dados enviados com sucesso para a Nuvem!";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro no envio: " + ex.Message;
            }
        }

        // Padrão do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}



        
    
