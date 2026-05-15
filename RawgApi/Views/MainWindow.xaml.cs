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
    }
}