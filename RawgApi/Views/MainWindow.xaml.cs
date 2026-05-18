using RawgApi.ViewModels;
using System.Windows;

namespace RawgApi.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void AbrirBancoLocal_Click(object sender, RoutedEventArgs e)
        {
            BancoLocalWindow telaBanco = new BancoLocalWindow();
            telaBanco.ShowDialog();
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}