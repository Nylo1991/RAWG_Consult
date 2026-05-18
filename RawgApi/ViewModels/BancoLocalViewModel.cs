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
        private readonly LocalDbContex _dbContext;
        private readonly Aluno2ApiService _aluno2ApiService;

        private bool _todosSelecionados;

        public ObservableCollection<Games> JogosSalvos { get; set; }

        private Games? _jogoSelecionado;
        public Games? JogoSelecionado
        {
            get => _jogoSelecionado;
            set
            {
                _jogoSelecionado = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand CarregarBancoCommand { get; }
        public ICommand AtualizarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand ExcluirMarcadosCommand { get; }
        public ICommand SelecionarTodosCommand { get; }
        public ICommand EnviarApiCommand { get; }

        public BancoLocalViewModel()
        {
            _dbContext = new LocalDbContex();
            _aluno2ApiService = new Aluno2ApiService();

            _dbContext.Database.EnsureCreated();

            JogosSalvos = new ObservableCollection<Games>();

            CarregarBancoCommand = new RelayCommand((o) => CarregarBanco());
            AtualizarCommand = new RelayCommand((o) => AtualizarJogo());
            ExcluirCommand = new RelayCommand((o) => ExcluirJogoSelecionado());
            ExcluirMarcadosCommand = new RelayCommand((o) => ExcluirJogosMarcados());
            SelecionarTodosCommand = new RelayCommand((o) => AlternarSelecaoTodos());
            EnviarApiCommand = new RelayCommand(async (o) => await EnviarParaApi());

            StatusMessage = "Clique em Carregar Banco para visualizar os jogos salvos.";
        }

        private void CarregarBanco()
        {
            try
            {
                var jogos = _dbContext.Games
                    .AsNoTracking()
                    .OrderByDescending(g => g.Upload)
                    .ToList();

                JogosSalvos.Clear();

                int contador = 1;

                foreach (var jogo in jogos)
                {
                    jogo.IsSelected = false;
                    jogo.DisplayId = contador;
                    contador++;

                    JogosSalvos.Add(jogo);
                }

                JogoSelecionado = null;

                StatusMessage = $"Banco local carregado: {jogos.Count} jogo(s) salvo(s).";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao carregar banco local: " + ObterErroCompleto(ex);
            }
        }

        private void AtualizarJogo()
        {
            try
            {
                if (JogoSelecionado == null)
                {
                    StatusMessage = "Selecione um jogo para atualizar.";
                    return;
                }

                NormalizarJogo(JogoSelecionado);

                var jogoBanco = _dbContext.Games.FirstOrDefault(g => g.Id == JogoSelecionado.Id);

                if (jogoBanco == null)
                {
                    StatusMessage = "Jogo não encontrado no banco.";
                    return;
                }

                jogoBanco.Nome = JogoSelecionado.Nome;
                jogoBanco.Descricao = JogoSelecionado.Descricao;
                jogoBanco.ImagemUrl = JogoSelecionado.ImagemUrl;
                jogoBanco.Avaliacao = JogoSelecionado.Avaliacao;
                jogoBanco.Classificacao = JogoSelecionado.Classificacao;
                jogoBanco.Upload = DateTime.Now;

                _dbContext.SaveChanges();

                CarregarBanco();

                StatusMessage = "Jogo atualizado com sucesso.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao atualizar: " + ObterErroCompleto(ex);
            }
        }

        private void ExcluirJogoSelecionado()
        {
            try
            {
                if (JogoSelecionado == null)
                {
                    StatusMessage = "Selecione um jogo para excluir.";
                    return;
                }

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

                var jogoBanco = _dbContext.Games.FirstOrDefault(g => g.Id == JogoSelecionado.Id);

                if (jogoBanco != null)
                {
                    _dbContext.Games.Remove(jogoBanco);
                    _dbContext.SaveChanges();
                }

                JogosSalvos.Remove(JogoSelecionado);
                JogoSelecionado = null;

                StatusMessage = "Jogo excluído do banco local com sucesso.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao excluir: " + ObterErroCompleto(ex);
            }
        }

        private void ExcluirJogosMarcados()
        {
            try
            {
                var selecionados = JogosSalvos.Where(g => g.IsSelected).ToList();

                if (!selecionados.Any())
                {
                    StatusMessage = "Marque pelo menos um jogo para excluir.";
                    return;
                }

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
                    var jogoBanco = _dbContext.Games.FirstOrDefault(g => g.Id == jogo.Id);

                    if (jogoBanco != null)
                    {
                        _dbContext.Games.Remove(jogoBanco);
                    }
                }

                _dbContext.SaveChanges();

                CarregarBanco();

                StatusMessage = $"{selecionados.Count} jogo(s) excluído(s) do banco local.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao excluir marcados: " + ObterErroCompleto(ex);
            }
        }

        private void AlternarSelecaoTodos()
        {
            _todosSelecionados = !_todosSelecionados;

            foreach (var jogo in JogosSalvos)
            {
                jogo.IsSelected = _todosSelecionados;
            }

            JogosSalvos = new ObservableCollection<Games>(JogosSalvos);
            OnPropertyChanged(nameof(JogosSalvos));

            StatusMessage = _todosSelecionados
                ? "Todos os jogos foram selecionados."
                : "Seleção removida.";
        }

        private async Task EnviarParaApi()
        {
            try
            {
                var selecionados = JogosSalvos.Where(g => g.IsSelected).ToList();

                if (!selecionados.Any() && JogoSelecionado != null)
                {
                    selecionados.Add(JogoSelecionado);
                }

                if (!selecionados.Any())
                {
                    StatusMessage = "Selecione ou marque pelo menos um jogo para enviar.";
                    return;
                }

                int sucessos = 0;
                int falhas = 0;

                foreach (var jogo in selecionados)
                {
                    NormalizarJogo(jogo);

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}