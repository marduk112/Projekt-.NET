using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
    /// Logika interakcji dla klasy Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public Chat()
        {
            InitializeComponent();
            rtxtDialogueWindow.Document.Blocks.Clear();
            this.DataContext = new ChatViewModel();
        }
       
        //private Thread _thread;
        //public string RecipientNick { get; private set; }

        /*private void DownloadFriendsList()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<UsersList>().As<IUsersList>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var reqUserList = new UserListReq { Login = Const.User.Login };
                var response = scope.Resolve<IUsersList>();
                foreach (var user in response.GetFriendsListWithPresenceStatus(reqUserList).Users)
                {
                    _friendsViewModel.Friends.Add(user);
                }
            }
        }*/
        /*private void StartListeningThread(SynchronizationContext ctx)
        {
            _thread = new Thread(() =>
            {
                Listening.Start();
                while (true)
                {
                    var activityResponse = Listening.ListeningActivity();
                    ctx.Post(_ =>
                    {
                        //ten kod jest wykonany w watku UI
                    }, null);
                    var presenceStatusResponse = Listening.ListeningPresenceStatus();
                    ctx.Post(_ => _friendsViewModel.Friends.Add(presenceStatusResponse), null);
                    var messageResponse = Listening.ListeningMessages();
                    ctx.Post(_ =>
                    {
                        //ten kod jest wykonany w watku UI

                    }, null);
                    //add info; activity status
                }
            }) { IsBackground = true };
        }*/
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //send presence status as offline
            /*Const.User.Status = Common.PresenceStatus.Offline;

            var builder = new ContainerBuilder();
            builder.Register(c => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<IPresenceStatus>();
                writer.SendPresenceStatus(Const.User);
            }

            //_thread.Abort();
            Close();*/
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
       

        private void IfCheckedI(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontStyle = FontStyles.Italic;
        }

        private void IfCheckedB(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontWeight = FontWeights.Bold;
        }

        private void IfUncheckedB(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontWeight = FontWeights.Normal;
        }

        private void IfUncheckedI(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontStyle = FontStyles.Normal;
        }
    }
}