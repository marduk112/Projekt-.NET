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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Autofac;
using Client.Interfaces;
using Client.Modules;
using Client.ViewModel;
using Common;
using RabbitMQ.Client;

namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
        }

        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
  
        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            //(new Contacts()).Show();
            var request = new AuthRequest {Login = txtLogin.Text, Password = pswbPassword.Password};
            AuthResponse response;

            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Login>().As<ILogin>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<ILogin>();
                response = writer.LoginAuthRequestResponse(request);
            }
           
            if (response.Status == Status.OK)
            {
                Const.User.Login = txtLogin.Text;
                Const.User.Status = Common.PresenceStatus.Online;
                
                using (var scope = container.BeginLifetimeScope())
                {
                    var writer = scope.Resolve<IPresenceStatus>();
                    writer.SendPresenceStatus(Const.User);
                }

                new Chat().Show();
                this.Close();
            }
            else
                MessageBox.Show(response.Message);
        }

    }

}