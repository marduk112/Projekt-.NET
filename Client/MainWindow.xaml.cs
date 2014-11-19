using System.Windows;
using Client.Modules;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reg = new Registration();
            var response = reg.registration(loginTextBox.Text, passwordTextBox.Text);
            MessageBox.Show(response.Message);
        }
    }
}
