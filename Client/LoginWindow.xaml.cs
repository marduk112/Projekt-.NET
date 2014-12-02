using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Client.Modules;
using Common;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public AuthResponse AuthResponse = new AuthResponse();
        public string Login;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            var authRequest = new AuthRequest {Login = LoginTextBox.Text, Password = PasswordTextBox.Password};
            AuthResponse = login.LoginAuthRequestResponse(authRequest);
            MessageBox.Show(AuthResponse.Message);
            if (AuthResponse.Status != Status.OK) return;
            Login = LoginTextBox.Text;
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
        }
    }
}
