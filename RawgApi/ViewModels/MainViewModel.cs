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
        // ==========================================
        // 1. CAMPOS PRIVADOS (Serviços e banco)
        // ==========================================
        // Serviço responsável por buscar jogos na API RAWG.
        private readonly RawgApiService _rawgApiService;

        // Contexto do banco de dados local SQLite.
        private readonly LocalDbContex _dbContext;

        // ==========================================
        // 2. PROPRIEDADE DE PESQUISA
        // ==========================================
        // Armazena o texto digitado pelo usuário no campo de busca.
        private string _termPesquisa;
        public string TermPesquisa
        {
            get => _termPesquisa;
            set
            {
                _termPesquisa = value;

                // Notifica a tela que o valor da propriedade foi alterado.
                OnPropertyChanged();
            }
        }

        // ==========================================
        // 3. MENSAGEM DE STATUS DA TELA
        // ==========================================
        // Armazena mensagens exibidas ao usuário, como erros, avisos e confirmações.
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;

                // Atualiza a mensagem exibida na interface.
                OnPropertyChanged();
            }
        }

        // ==========================================
        // 4. JOGO SELECIONADO NA TABELA
        // ==========================================
        // Guarda o jogo selecionado pelo usuário no DataGrid.
        private Games _selectedGame;
        public Games SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;

                // Notifica a interface quando outro jogo é selecionado.
                OnPropertyChanged();
            }
        }

        // ==========================================
        // 5. LISTA DE JOGOS EXIBIDA NA TELA
        // ==========================================
        // ObservableCollection permite que a tabela atualize automaticamente
        // quando itens são adicionados ou removidos.
        public ObservableCollection<Games> ListaGames { get; set; }

        // ==========================================
        // 6. COMANDOS DOS BOTÕES
        // ==========================================
        // Comando usado pelo botão Buscar RAWG.
        public ICommand FetchFromRawgCommand { get; }

        // Comando usado pelo botão Salvar Selecionado.
        public ICommand SaveToLocalDbCommand { get; }

        // ==========================================
        // 7. CONSTRUTOR DA VIEWMODEL
        // ==========================================
        // Inicializa serviços, banco, lista e comandos usados pela tela principal.
        public MainViewModel()
        {
            // Instancia o serviço responsável pela comunicação com a API RAWG.
            _rawgApiService = new RawgApiService();

            // Instancia o contexto do banco local SQLite.
            _dbContext = new LocalDbContex();

            // Inicializa a lista que será exibida no DataGrid.
            ListaGames = new ObservableCollection<Games>();

            // Garante que o banco local seja criado ao iniciar o programa.
            InicializarBanco();

            // Liga o botão Buscar RAWG ao método ProcurarNaRawg.
            FetchFromRawgCommand = new RelayCommand(async (o) => await ProcurarNaRawg());

            // Liga o botão Salvar Selecionado ao método SalvarLocal.
            SaveToLocalDbCommand = new RelayCommand((o) => SalvarLocal());

            // Tela principal inicia limpa.
            TermPesquisa = string.Empty;
            SelectedGame = null;
            StatusMessage = "Digite o nome de um jogo para pesquisar.";
        }

        // ==========================================
        // 8. INICIALIZAÇÃO DO BANCO LOCAL
        // ==========================================
        // Cria o banco SQLite caso ele ainda não exista.
        private void InicializarBanco()
        {
            try
            {
                // Garante a criação do banco e das tabelas configuradas no DbContext.
                _dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                // Exibe uma mensagem caso ocorra erro na criação ou acesso ao banco.
                MessageBox.Show(
                    "Erro ao iniciar banco SQLite:\n\n" + ObterErroCompleto(ex),
                    "Erro SQLite",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ==========================================
        // 9. BUSCA DE JOGOS NA API RAWG
        // ==========================================
        // Realiza a pesquisa de jogos de acordo com o termo digitado pelo usuário.
        private async Task ProcurarNaRawg()
        {
            // Verifica se o campo de pesquisa está vazio.
            if (string.IsNullOrWhiteSpace(TermPesquisa))
            {
                StatusMessage = "Digite um termo para pesquisar.";
                return;
            }

            try
            {
                StatusMessage = "Buscando na RAWG...";

                // Chama o serviço responsável por consultar a API RAWG.
                var resultados = await _rawgApiService.BuscarJogosAsync(TermPesquisa);

                // Limpa os resultados anteriores da tabela.
                ListaGames.Clear();

                // Adiciona os jogos encontrados na lista exibida pela tela.
                foreach (var jogo in resultados)
                {
                    // Garante que nenhum jogo venha previamente marcado.
                    jogo.IsSelected = false;

                    // Adiciona o jogo na tabela.
                    ListaGames.Add(jogo);
                }

                // Remove qualquer seleção anterior.
                SelectedGame = null;

                StatusMessage = $"{resultados.Count} jogo(s) encontrado(s). Selecione um jogo para salvar.";
            }
            catch (Exception ex)
            {
                // Exibe erro caso a busca na API falhe.
                StatusMessage = "Erro ao buscar: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 10. SALVAMENTO DO JOGO NO BANCO LOCAL
        // ==========================================
        // Salva no SQLite apenas o jogo selecionado ou marcado na tabela.
        private void SalvarLocal()
        {
            try
            {
                // Primeiro tenta pegar o jogo selecionado na linha da tabela.
                Games jogo = SelectedGame;

                // Caso nenhuma linha esteja selecionada, tenta pegar um jogo marcado no checkbox.
                if (jogo == null)
                {
                    jogo = ListaGames.FirstOrDefault(g => g.IsSelected);
                }

                // Se nenhum jogo foi selecionado ou marcado, exibe aviso ao usuário.
                if (jogo == null)
                {
                    StatusMessage = "Selecione uma linha ou marque um jogo para salvar.";
                    return;
                }

                // Evita salvar jogos inválidos.
                if (jogo.Id <= 0)
                {
                    StatusMessage = "Jogo inválido para salvar.";
                    return;
                }

                // Corrige campos nulos ou vazios antes de salvar.
                NormalizarJogo(jogo);

                // Verifica se o jogo já existe no banco para evitar duplicidade.
                bool jaExiste = _dbContext.Games.Any(g => g.Id == jogo.Id);

                if (jaExiste)
                {
                    StatusMessage = $"O jogo '{jogo.Nome}' já está salvo no SQLite.";
                    return;
                }

                // Cria um novo objeto para ser salvo no banco local.
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

                // Adiciona o jogo no contexto do banco.
                _dbContext.Games.Add(novoJogo);

                // Salva definitivamente no SQLite.
                _dbContext.SaveChanges();

                StatusMessage = $"Jogo '{jogo.Nome}' salvo com sucesso no SQLite.";
            }
            catch (DbUpdateException ex)
            {
                // Trata erros específicos do Entity Framework ao salvar no banco.
                StatusMessage = "Erro ao salvar no SQLite: " + ObterErroCompleto(ex);
            }
            catch (Exception ex)
            {
                // Trata qualquer outro erro inesperado.
                StatusMessage = "Erro inesperado ao salvar: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 11. NORMALIZAÇÃO DOS DADOS DO JOGO
        // ==========================================
        // Evita erro de valores nulos antes de salvar ou manipular o jogo.
        private void NormalizarJogo(Games jogo)
        {
            // Garante que campos de texto não sejam nulos.
            jogo.Nome = jogo.Nome ?? string.Empty;
            jogo.Descricao = jogo.Descricao ?? string.Empty;
            jogo.ImagemUrl = jogo.ImagemUrl ?? string.Empty;

            // Garante valores padrão para avaliação e classificação.
            jogo.Avaliacao = string.IsNullOrWhiteSpace(jogo.Avaliacao) ? "0" : jogo.Avaliacao;
            jogo.Classificacao = string.IsNullOrWhiteSpace(jogo.Classificacao) ? "0" : jogo.Classificacao;

            // Caso a data esteja vazia, define a data atual.
            if (jogo.Upload == default)
            {
                jogo.Upload = DateTime.Now;
            }
        }

        // ==========================================
        // 12. TRATAMENTO DE ERROS
        // ==========================================
        // Percorre a exceção e suas exceções internas para montar uma mensagem completa.
        private string ObterErroCompleto(Exception ex)
        {
            var mensagem = new StringBuilder();

            // Captura a mensagem principal e todas as InnerExceptions.
            while (ex != null)
            {
                mensagem.AppendLine(ex.Message);
                ex = ex.InnerException;
            }

            return mensagem.ToString();
        }

        // ==========================================
        // 13. NOTIFICAÇÃO DE ALTERAÇÃO DE PROPRIEDADES
        // ==========================================
        // Evento usado pelo WPF para atualizar a interface quando uma propriedade muda.
        public event PropertyChangedEventHandler PropertyChanged;

        // Método padrão do INotifyPropertyChanged.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}