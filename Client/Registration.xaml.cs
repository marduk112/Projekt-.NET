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
using Autofac;
using Client.Interfaces;
using Client.Modules;
using Common;
using RabbitMQ.Client;

namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Ellipse_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void LblName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnRegistry1_Click(object sender, RoutedEventArgs e)
        {
            if (pswbPasswordReg.Password.Equals(pswbPasswordRegRepeat.Password))
            {
                var request = new CreateUserReq {Login = txtLogin1.Text, Password = pswbPasswordReg.Password};

                CreateUserResponse response;
                var builder = new ContainerBuilder();
                builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
                builder.RegisterType<Modules.Registration>().As<IRegistration>();
                var container = builder.Build();
                using (var scope = container.BeginLifetimeScope())
                {
                    var writer = scope.Resolve<IRegistration>();
                    response = writer.registration(request);
                }
                
                if (response.Status == Status.OK)
                    Close();
                else
                    MessageBox.Show(response.Message);
            }
            else
                MessageBox.Show("Error");
        }
    }
}
