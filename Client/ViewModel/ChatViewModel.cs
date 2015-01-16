using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Autofac;
using Client.Annotations;
using Client.Interfaces;
using Client.Modules;
using Client.RichTextBoxEmoticons;
using Common;
using Microsoft.Practices.Prism.Commands;
using RabbitMQ.Client;
using Xceed.Wpf.DataGrid.Views;
using PresenceStatus = Common.PresenceStatus;

namespace Client.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged  
    {
        public DelegateCommand SelectForm { get; private set; }
        public DelegateCommand ViewEmoticons { get; private set; }
        public DelegateCommand AddAttachment { get; private set; }
        public DelegateCommand Close { get; private set; }
        public DelegateCommand<object[]> AddEmoticon { get; private set; }
        public DelegateCommand FontVisibility { get; private set; }
        public DelegateCommand AddFriend { get; private set; }
        public DelegateCommand SendMessage { get; private set; }
        public ObservableCollection<User> Friends { get; set; }
        public ObservableCollection<PresenceStatusView> PresenceStatuses { get; set; } 

        public ChatViewModel()
        {
            SelectForm = new DelegateCommand(doSelectForm);
            ViewEmoticons = new DelegateCommand(viewEmoticons);
            AddAttachment = new DelegateCommand(addAttachment);
            Close = new DelegateCommand(closeApplication);
            AddEmoticon = new DelegateCommand<object[]>(addEmoticon);
            FontVisibility = new DelegateCommand(fontVisibility);
            Friends = new ObservableCollection<User>();
            PresenceStatuses = new ObservableCollection<PresenceStatusView>();
            AddFriend = new DelegateCommand(addFriend, canAddFriend);
            SendMessage = new DelegateCommand(sendMessage, canSendMessage);
            AddPresenceStatuses();
        }

        public Visibility ChatSwitchMode 
        {                              
            get { return _chatSwitchMode; }
            set
            {
                _chatSwitchMode = value;
                OnPropertyChanged();
                SelectForm.RaiseCanExecuteChanged();
            }
        }
        public Visibility ChatSwitchMode2
        {
            get { return _chatSwitchMode2; }
            set
            {
                _chatSwitchMode2 = value;
                OnPropertyChanged();
                SelectForm.RaiseCanExecuteChanged();
            }
        }

        public Visibility EmoticonsVisibility
        {
            get { return _emoticonsVisibility; }
            set
            {
                _emoticonsVisibility = value;
                OnPropertyChanged();
                ViewEmoticons.RaiseCanExecuteChanged();
            }
        }

        public Visibility Font
        {
            get { return _font; }
            set
            {
                _font = value;
                OnPropertyChanged();
                ViewEmoticons.RaiseCanExecuteChanged();
            }
        }

        public PresenceStatus PresenceStatus
        {
            get { return _presenceStatus; }
            set
            {
                _presenceStatus = value;
                OnPropertyChanged();
                Const.User.Status = _presenceStatus;
                try
                {
                    var builder = new ContainerBuilder();
                    builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
                    builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
                    var container = builder.Build();
                    using (var scope = container.BeginLifetimeScope())
                    {
                        var writer = scope.Resolve<IPresenceStatus>();
                        writer.SendPresenceStatus(Const.User);
                    }
                }
                catch { }
            }
        }

        public string FriendLogin
        {
            get { return _friendLogin; }
            set
            {
                _friendLogin = value;
                OnPropertyChanged();
                AddFriend.RaiseCanExecuteChanged();
            }
        }

        public string UserName
        {
            get { return Const.User.Login; }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
                SendMessage.RaiseCanExecuteChanged();
            }
        }

        public User Friend
        {
            get { return _friend; }
            set
            {
                _friend = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private Visibility _chatSwitchMode = Visibility.Visible;
        private Visibility _emoticonsVisibility = Visibility.Collapsed;
        private Visibility _font = Visibility.Collapsed;
        private Visibility _chatSwitchMode2 = Visibility.Collapsed;
        private PresenceStatus _presenceStatus = PresenceStatus.Online;
        private string _friendLogin;
        private string _message;
        private string _pathToAttachment;
        private User _friend;
        private List<string> _allUsersList = new List<string>();
        
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void addFriend()
        {
            var user = new User {Login = FriendLogin};
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<IPresenceStatus>();
                //download user presence status
                //writer.
            }
            Friends.Add(user);
        }

        private void sendMessage()
        {
            try
            {
                var message = new MessageReq
                {
                    Login = Const.User.Login,
                    Message = Message,
                    Recipient = Friend.Login,
                    SendTime = new DateTimeOffset().LocalDateTime
                };
                var builder = new ContainerBuilder();
                builder.Register(_ => new ConnectionFactory {HostName = Const.HostName}).As<IConnectionFactory>();
                builder.RegisterType<Messages>().As<IMessages>();
                var container = builder.Build();
                using (var scope = container.BeginLifetimeScope())
                {
                    var writer = scope.Resolve<IMessages>();
                    writer.SendMessage(message);
                }
            }
            catch { }
        }

        private bool canSendMessage()
        {
            return !string.IsNullOrEmpty(Message);
        }

        private bool canAddFriend()
        {
            return _allUsersList.Any(login => login.Equals(FriendLogin));
        }
        private void doSelectForm()
        {
            switch (ChatSwitchMode)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    ChatSwitchMode = Visibility.Visible;
                    ChatSwitchMode2 = Visibility.Collapsed;
                    break;
                case Visibility.Visible:
                    ChatSwitchMode = Visibility.Collapsed;
                    ChatSwitchMode2 = Visibility.Visible;
                    DownloadUsersList();
                    break;
                default:
                    break;
            }
        }

        private void viewEmoticons()
        {
            switch (EmoticonsVisibility)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    EmoticonsVisibility = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    EmoticonsVisibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void fontVisibility()
        {
            switch (Font)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    Font = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    Font = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }
        private void addAttachment()
        {
            var window = new Microsoft.Win32.OpenFileDialog
            {
                AddExtension = true,
                Multiselect = false
            };
            window.ShowDialog();
            _pathToAttachment = window.FileName;
        }

        private void closeApplication()
        {
            Application.Current.Shutdown();
        }

        private void addEmoticon(object[] values)
        {
            
            var textDialog = values[0] as TextBox;
            var emoticon = values[1] as string;
            textDialog.Text += " " + emoticon + " ";
        }


        private void AddPresenceStatuses()
        {
            PresenceStatuses.Add(new PresenceStatusView { PresenceStatus = PresenceStatus.Online, Value = "Online" });
            PresenceStatuses.Add(new PresenceStatusView { PresenceStatus = PresenceStatus.Afk, Value = "Away from keyboard" });
            PresenceStatuses.Add(new PresenceStatusView { PresenceStatus = PresenceStatus.Offline, Value = "Offline" });
        }
        private void DownloadUsersList()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.Register(_ => new ConnectionFactory {HostName = Const.HostName}).As<IConnectionFactory>();
                builder.RegisterType<UsersList>().As<IUsersList>();
                var container = builder.Build();
                using (var scope = container.BeginLifetimeScope())
                {
                    var reqUserList = new UserListReq {Login = Const.User.Login};
                    var response = scope.Resolve<IUsersList>();
                    foreach (var user in response.UserListReqResponse(reqUserList).Users)
                    {
                        _allUsersList.Add(user.Login);
                    }
                }
            }
            catch{}
        }
    }

    public class PresenceStatusView
    {
        public PresenceStatus PresenceStatus { get; set; }
        public string Value { get; set; }
    }
}
