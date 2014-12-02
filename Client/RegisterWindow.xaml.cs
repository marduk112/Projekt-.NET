using System;
using System.Collections.Generic;
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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordRepeatTextBox.Password == PasswordTextBox.Password)
            {
                var register = new Registration();
                var createUserReq = new CreateUserReq();
                createUserReq.Login = LoginTextBox.Text;
                createUserReq.Password = PasswordTextBox.Password;
                var response = register.registration(createUserReq);
                MessageBox.Show(response.Message);
                if (response.Status == Status.OK)
                    this.Close();
            }
            else
                MessageBox.Show("Password is not properly prescribed");
        }
    }
}
