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
    public class BancoLocalViewModel : INotifyPropertyChanged
    {
        // ==========================================
        // 1. CAMPOS PRIVADOS (Banco, API e controle)
        // ==========================================

        // Contexto responsável pelo acesso ao banco de dados local SQLite.
        private readonly LocalDbContex _dbContext;

        // Serviço responsável pelo envio dos jogos para a API externa.
        private readonly Aluno2ApiService _aluno2ApiService;

        // Controla se todos os jogos da tabela estão selecionados ou não.
        private bool _todosSelecionados;

        // ==========================================
        // 2. LISTA DE JOGOS SALVOS
        // ==========================================

        // Lista observável usada para exibir os jogos salvos no banco local.
        // Como é ObservableCollection, o DataGrid atualiza automaticamente ao alterar a lista.
        public ObservableCollection<Games> JogosSalvos { get; set; }

        // ==========================================
        // 3. JOGO SELECIONADO NA TABELA
        // ==========================================

        // Armazena o jogo selecionado pelo usuário na tela Meu Banco Local.
        private Games? _jogoSelecionado;
        public Games? JogoSelecionado
        {
            get => _jogoSelecionado;
            set
            {
                _jogoSelecionado = value;

                // Notifica a interface quando o jogo selecionado muda.
                OnPropertyChanged();
            }
        }

        // ==========================================
        // 4. MENSAGEM DE STATUS
        // ==========================================

        // Mensagem exibida na parte inferior da tela para orientar o usuário.
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;

                // Atualiza a mensagem na interface.
                OnPropertyChanged();
            }
        }

        // ==========================================
        // 5. COMANDOS DOS BOTÕES
        // ==========================================

        // Comando do botão Carregar Banco.
        public ICommand CarregarBancoCommand { get; }

        // Comando do botão Atualizar.
        public ICommand AtualizarCommand { get; }

        // Comando do botão Excluir.
        public ICommand ExcluirCommand { get; }

        // Comando do botão Excluir Marcados.
        public ICommand ExcluirMarcadosCommand { get; }

        // Comando do botão Selecionar Todos.
        public ICommand SelecionarTodosCommand { get; }

        // Comando do botão Enviar API.
        public ICommand EnviarApiCommand { get; }

        // ==========================================
        // 6. CONSTRUTOR DA VIEWMODEL
        // ==========================================

        // Inicializa banco, serviço de API, lista e comandos da tela Meu Banco Local.
        public BancoLocalViewModel()
        {
            // Cria o contexto do banco local SQLite.
            _dbContext = new LocalDbContex();

            // Cria o serviço responsável pelo envio para a API externa.
            _aluno2ApiService = new Aluno2ApiService();

            try
            {
                // Garante que o banco local exista.
                _dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao iniciar banco SQLite:\n\n" + ex.Message,
                    "Erro SQLite",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            // Inicializa a lista que será exibida no DataGrid.
            JogosSalvos = new ObservableCollection<Games>();

            // Liga cada botão da tela ao seu respectivo método.
            CarregarBancoCommand = new RelayCommand((o) => CarregarBanco());
            AtualizarCommand = new RelayCommand((o) => AtualizarJogo());
            ExcluirCommand = new RelayCommand((o) => ExcluirJogoSelecionado());
            ExcluirMarcadosCommand = new RelayCommand((o) => ExcluirJogosMarcados());
            SelecionarTodosCommand = new RelayCommand((o) => AlternarSelecaoTodos());
            EnviarApiCommand = new RelayCommand(async (o) => await EnviarParaApi());

            // Mensagem inicial exibida quando a tela é aberta.
            StatusMessage = "Clique em Carregar Banco para visualizar os jogos salvos.";
        }

        // ==========================================
        // 7. CARREGAMENTO DO BANCO LOCAL
        // ==========================================

        // Carrega os jogos salvos no SQLite e exibe na tabela da tela Meu Banco Local.
        private void CarregarBanco()
        {
            try
            {
                // Busca os jogos do banco sem rastreamento e ordena pelo upload mais recente.
                var jogos = _dbContext.Games
                    .AsNoTracking()
                    .OrderByDescending(g => g.Upload)
                    .ToList();

                // Limpa a tabela antes de carregar os dados novamente.
                JogosSalvos.Clear();

                // Contador usado para exibir ID local sequencial: 1, 2, 3, 4...
                int contador = 1;

                foreach (var jogo in jogos)
                {
                    // Garante que nenhum jogo venha marcado ao carregar.
                    jogo.IsSelected = false;

                    // Define o ID local apenas para exibição na tela.
                    jogo.DisplayId = contador;
                    contador++;

                    // Adiciona o jogo na lista exibida pelo DataGrid.
                    JogosSalvos.Add(jogo);
                }

                // Remove qualquer seleção anterior.
                JogoSelecionado = null;

                if (jogos.Count == 0)
                {
                    StatusMessage = "Nenhum jogo salvo no banco local.";
                    return;
                }

                StatusMessage = $"Banco local carregado: {jogos.Count} jogo(s) salvo(s).";
            }
            catch (DbUpdateException ex)
            {
                StatusMessage = "Erro ao acessar o banco local: " + ObterErroCompleto(ex);
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao carregar banco local: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 8. ATUALIZAÇÃO DE JOGO
        // ==========================================

        // Atualiza no banco local as informações do jogo selecionado.
        private void AtualizarJogo()
        {
            try
            {
                // Verifica se existe um jogo selecionado.
                if (JogoSelecionado == null)
                {
                    StatusMessage = "Selecione um jogo para atualizar.";
                    return;
                }

                if (JogoSelecionado.Id <= 0)
                {
                    StatusMessage = "Jogo inválido para atualizar.";
                    return;
                }

                // Corrige valores nulos ou vazios antes de salvar.
                NormalizarJogo(JogoSelecionado);

                // Busca no banco o jogo correspondente ao ID original da RAWG.
                var jogoBanco = _dbContext.Games.FirstOrDefault(g => g.Id == JogoSelecionado.Id);

                if (jogoBanco == null)
                {
                    StatusMessage = "Jogo não encontrado no banco.";
                    return;
                }

                // Atualiza os campos do registro encontrado no banco.
                jogoBanco.Nome = JogoSelecionado.Nome;
                jogoBanco.Descricao = JogoSelecionado.Descricao;
                jogoBanco.ImagemUrl = JogoSelecionado.ImagemUrl;
                jogoBanco.Avaliacao = JogoSelecionado.Avaliacao;
                jogoBanco.Classificacao = JogoSelecionado.Classificacao;
                jogoBanco.Upload = DateTime.Now;

                // Salva as alterações no SQLite.
                _dbContext.SaveChanges();

                // Recarrega a tabela para exibir os dados atualizados.
                CarregarBanco();

                StatusMessage = "Jogo atualizado com sucesso.";
            }
            catch (DbUpdateException ex)
            {
                StatusMessage = "Erro ao atualizar no banco local: " + ObterErroCompleto(ex);
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao atualizar: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 9. EXCLUSÃO DE JOGO SELECIONADO
        // ==========================================

        // Exclui do banco local o jogo selecionado na tabela.
        private void ExcluirJogoSelecionado()
        {
            try
            {
                // Verifica se o usuário selecionou algum jogo.
                if (JogoSelecionado == null)
                {
                    StatusMessage = "Selecione um jogo para excluir.";
                    return;
                }

                if (JogoSelecionado.Id <= 0)
                {
                    StatusMessage = "Jogo inválido para excluir.";
                    return;
                }

                // Solicita confirmação antes de excluir o jogo.
                var confirmacao = MessageBox.Show(
                    $"Deseja excluir o jogo '{JogoSelecionado.Nome}'?",
                    "Confirmação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (confirmacao != MessageBoxResult.Yes)
                {
                    return;
                }

                // Localiza o jogo no banco pelo ID original da RAWG.
                var jogoBanco = _dbContext.Games.FirstOrDefault(g => g.Id == JogoSelecionado.Id);

                if (jogoBanco != null)
                {
                    // Remove o jogo do banco e salva a alteração.
                    _dbContext.Games.Remove(jogoBanco);
                    _dbContext.SaveChanges();
                }

                // Remove também da lista exibida na tela.
                JogosSalvos.Remove(JogoSelecionado);
                JogoSelecionado = null;

                StatusMessage = "Jogo excluído do banco local com sucesso.";
            }
            catch (DbUpdateException ex)
            {
                StatusMessage = "Erro ao excluir do banco local: " + ObterErroCompleto(ex);
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao excluir: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 10. EXCLUSÃO DE JOGOS MARCADOS
        // ==========================================

        // Exclui todos os jogos marcados pelo checkbox.
        private void ExcluirJogosMarcados()
        {
            try
            {
                // Busca todos os jogos marcados na tabela.
                var selecionados = JogosSalvos.Where(g => g.IsSelected).ToList();

                if (!selecionados.Any())
                {
                    StatusMessage = "Marque pelo menos um jogo para excluir.";
                    return;
                }

                // Solicita confirmação antes de excluir vários registros.
                var confirmacao = MessageBox.Show(
                    $"Deseja excluir {selecionados.Count} jogo(s) do banco local?",
                    "Confirmação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (confirmacao != MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var jogo in selecionados)
                {
                    // Localiza cada jogo no banco pelo ID original.
                    var jogoBanco = _dbContext.Games.FirstOrDefault(g => g.Id == jogo.Id);

                    if (jogoBanco != null)
                    {
                        // Remove o registro do contexto.
                        _dbContext.Games.Remove(jogoBanco);
                    }
                }

                // Salva todas as exclusões no SQLite.
                _dbContext.SaveChanges();

                // Recarrega a tabela após excluir os registros.
                CarregarBanco();

                StatusMessage = $"{selecionados.Count} jogo(s) excluído(s) do banco local.";
            }
            catch (DbUpdateException ex)
            {
                StatusMessage = "Erro ao excluir jogos marcados do banco: " + ObterErroCompleto(ex);
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao excluir marcados: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 11. SELECIONAR OU DESMARCAR TODOS
        // ==========================================

        // Alterna entre marcar todos os jogos e desmarcar todos.
        private void AlternarSelecaoTodos()
        {
            if (JogosSalvos == null || !JogosSalvos.Any())
            {
                StatusMessage = "Nenhum jogo carregado para selecionar.";
                return;
            }

            // Inverte o estado atual da seleção.
            _todosSelecionados = !_todosSelecionados;

            foreach (var jogo in JogosSalvos)
            {
                // Aplica o mesmo estado para todos os jogos.
                jogo.IsSelected = _todosSelecionados;
            }

            // Recria a coleção para forçar atualização visual do DataGrid.
            JogosSalvos = new ObservableCollection<Games>(JogosSalvos);
            OnPropertyChanged(nameof(JogosSalvos));

            StatusMessage = _todosSelecionados
                ? "Todos os jogos foram selecionados."
                : "Seleção removida.";
        }

        // ==========================================
        // 12. ENVIO PARA API EXTERNA
        // ==========================================

        // Envia para a API externa os jogos marcados ou o jogo selecionado.
        private async Task EnviarParaApi()
        {
            try
            {
                // Busca os jogos marcados pelo checkbox.
                var selecionados = JogosSalvos.Where(g => g.IsSelected).ToList();

                // Caso nenhum esteja marcado, tenta enviar o jogo selecionado na linha.
                if (!selecionados.Any() && JogoSelecionado != null)
                {
                    selecionados.Add(JogoSelecionado);
                }

                // Se não houver jogo marcado nem selecionado, interrompe o envio.
                if (!selecionados.Any())
                {
                    StatusMessage = "Selecione ou marque pelo menos um jogo para enviar.";
                    return;
                }

                int sucessos = 0;
                int falhas = 0;

                foreach (var jogo in selecionados)
                {
                    if (jogo == null || jogo.Id <= 0)
                    {
                        falhas++;
                        continue;
                    }

                    // Normaliza os dados antes do envio.
                    NormalizarJogo(jogo);

                    // Envia o jogo para a API externa.
                    bool ok = await _aluno2ApiService.EnviarJogoAsync(jogo);

                    if (ok)
                    {
                        sucessos++;
                    }
                    else
                    {
                        falhas++;
                    }
                }

                // Exibe o resultado final do envio.
                MessageBox.Show(
                    $"Envio concluído!\nSucessos: {sucessos}\nFalhas: {falhas}",
                    "Resultado API",
                    MessageBoxButton.OK,
                    falhas > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information
                );

                StatusMessage = $"Envio para API concluído. Sucessos: {sucessos}. Falhas: {falhas}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao enviar para API: " + ObterErroCompleto(ex);
            }
        }

        // ==========================================
        // 13. NORMALIZAÇÃO DOS DADOS
        // ==========================================

        // Garante que o jogo não tenha campos nulos ou inválidos antes de atualizar, excluir ou enviar.
        private void NormalizarJogo(Games jogo)
        {
            // Garante que campos de texto não fiquem nulos.
            jogo.Nome = jogo.Nome ?? string.Empty;
            jogo.Descricao = jogo.Descricao ?? string.Empty;
            jogo.ImagemUrl = jogo.ImagemUrl ?? string.Empty;

            // Garante valores padrão para campos numéricos armazenados como texto.
            jogo.Avaliacao = string.IsNullOrWhiteSpace(jogo.Avaliacao) ? "0" : jogo.Avaliacao;
            jogo.Classificacao = string.IsNullOrWhiteSpace(jogo.Classificacao) ? "0" : jogo.Classificacao;

            // Caso a data esteja vazia, define a data atual.
            if (jogo.Upload == default)
            {
                jogo.Upload = DateTime.Now;
            }
        }

        // ==========================================
        // 14. TRATAMENTO DE ERROS
        // ==========================================

        // Monta uma mensagem completa com a exceção principal e suas exceções internas.
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

        // ==========================================
        // 15. NOTIFICAÇÃO DE ALTERAÇÃO DE PROPRIEDADES
        // ==========================================

        // Evento utilizado pelo WPF para atualizar a interface quando uma propriedade muda.
        public event PropertyChangedEventHandler? PropertyChanged;

        // Método padrão do INotifyPropertyChanged.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}