using System;
using System.Threading.Tasks;
using System.Windows;

namespace WPFTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IDialogService dialogService;
        IFileService fileService;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel(this, dialogService, fileService);

        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Export exportWindow = new Export(this);
            exportWindow.ShowDialog();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            await RunReport();
        }

        private async Task RunReport()
        {
            //peopleList.Items.Clear();
            ApplicationViewModel applicationViewModel = new ApplicationViewModel(this, dialogService, fileService);
            await applicationViewModel.ReadCSVAsync();
        }
    }
}
