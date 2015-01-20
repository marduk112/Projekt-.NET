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
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public DelegateCommand Close { get; private set; }
        public DelegateCommand Minimize { get; private set; }
        public DelegateCommand SetLanguageEN { get; private set; }
        public DelegateCommand SetLanguagePL { get; private set; }
        public DelegateCommand OpenRegistrationWindow { get; private set; }
        //public DelegateCommand<object> LogIn { get; private set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public LoginViewModel()
        {
            Close = new DelegateCommand(CloseApplication);
            Minimize = new DelegateCommand(Minimizing);
            SetLanguageEN = new DelegateCommand(LanguageEN);
            SetLanguagePL = new DelegateCommand(LanguagePL);
            OpenRegistrationWindow = new DelegateCommand(OpenRegistration);
            // LogIn = new DelegateCommand<object> (SignIn);
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
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

        private void LanguagePL()
        {
            var cultureInfo = new System.Globalization.CultureInfo("pl");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            SetLanguageDictionary();
        }

        private void LanguageEN()
        {
            var cultureInfo = new System.Globalization.CultureInfo("en");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            SetLanguageDictionary();
        }

        private void SetLanguageDictionary()
        {
            var dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "pl":
                    dict.Source = new Uri("..\\Resources\\Resources.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
                case "en":
                    dict.Source = new Uri("..\\Resources\\Resources.EN.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\Resources.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
            }
        }

        private void OpenRegistration()
        {
            Registration window = new Registration();
            window.Show();
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


        private void SignIn(object obj)
        {
            var request = new AuthRequest { Login = Login, Password = ? };
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
                Const.User.Login = Login;
                Const.User.Status = Common.PresenceStatus.Online;

                using (var scope = container.BeginLifetimeScope())
                {
                    var writer = scope.Resolve<IPresenceStatus>();
                    writer.SendPresenceStatus(Const.User);
                }

                new Chat().Show();
                Window currentwindow=obj as Window;
                currentwindow.Close(); //poprawic- ma sie zamykac tylko okno
            }
            else
                MessageBox.Show(response.Message);
        } */

    }
}
