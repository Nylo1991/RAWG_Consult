using RawgApi.ViewModels;
using System.Windows;

namespace RawgApi.Views
{
    public partial class BancoLocalWindow : Window
    {
        public BancoLocalWindow()
        {
            InitializeComponent();
            DataContext = new BancoLocalViewModel();
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}