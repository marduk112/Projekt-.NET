using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Autofac;
using Client.Annotations;
using Client.Interfaces;
using Client.Modules;
using Common;
using Microsoft.Practices.Prism.Commands;
using RabbitMQ.Client;
using Xceed.Wpf.DataGrid.Views;
using System.Threading;
namespace Client.ViewModel
{
    public class RegistrationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public DelegateCommand<object> CloseWindow { get; private set; }
        public DelegateCommand Minimize { get; private set; }
        //public DelegateCommand<object> Register { get; private set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public RegistrationViewModel()
        {
            CloseWindow = new DelegateCommand<object>(ClosingWindow);
            Minimize = new DelegateCommand(Minimizing);
            //Register = new DelegateCommand<object> (SignUp);
        }

        private void ClosingWindow(object obj)
        {
            Window w = obj as Window;
            w.Close();
        }

        private void Minimizing()
        {
            State = WindowState.Minimized;
        }

        private WindowState _state;

        public WindowState State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        /*
        private string _Login;
         
        public string Login
        {
            get { return _Login; }
            set
            {
                _Login = value;
                OnPropertyChanged("Login");
            }
        }

        private void SignUp(object obj)
        {
            if (pswbPasswordReg.Password.Equals(pswbPasswordRegRepeat.Password))
            {
                var request = new CreateUserReq {Login = Login, Password = pswbPasswordReg.Password};

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
                {
                    Window wind=obj as Window;
                    wind.Close();
                }
                else
                    MessageBox.Show(response.Message);
            }
            else
                MessageBox.Show("Error");
        }
         */
    }
}
