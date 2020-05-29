using System.Windows;

namespace WPFTask
{
    /// <summary>
    /// Логика взаимодействия для NewPersonWindow.xaml
    /// </summary>
    public partial class NewPersonWindow : Window
    {
        public Person Person { get; private set; }
        public MainWindow mainWindow;
        
        public NewPersonWindow( Person person )
        {
            InitializeComponent();
            Person = person;
            DataContext = Person;
        }
        
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            mainWindow = new MainWindow();
        }
    }
}
