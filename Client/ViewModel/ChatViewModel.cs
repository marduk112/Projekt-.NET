using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
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
using System.Threading;

namespace Client.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged  
    {
        Listening listener = new Listening();

        public DelegateCommand ChangeFormVisibility { get; private set; }
        public DelegateCommand ViewEmoticons { get; private set; }
        public DelegateCommand AddAttachment { get; private set; }
        public DelegateCommand Close { get; private set; }
        public DelegateCommand<string> AddEmoticon { get; private set; }
        public DelegateCommand FontVisibility { get; private set; }
        public DelegateCommand AddFriend { get; private set; }
        public DelegateCommand SendMessage { get; private set; }
        public DelegateCommand RemoveFriend { get; private set; }
        public ObservableCollection<User> Friends { get; set; }
        public ObservableCollection<PresenceStatusView> PresenceStatuses { get; set; }
        public ObservableCollection<MessageNotification> Conversation { get; set; } 
        
        public ChatViewModel()
        {
            ChangeFormVisibility = new DelegateCommand(doSelectForm);
            ViewEmoticons = new DelegateCommand(viewEmoticons);
            AddAttachment = new DelegateCommand(addAttachment);
            Close = new DelegateCommand(closeApplication);
            AddEmoticon = new DelegateCommand<string>(addEmoticon);
            FontVisibility = new DelegateCommand(fontVisibility);
            Friends = new ObservableCollection<User>();
            PresenceStatuses = new ObservableCollection<PresenceStatusView>();
            AddFriend = new DelegateCommand(addFriend, canAddFriend);
            SendMessage = new DelegateCommand(sendMessage, canSendMessage);
            RemoveFriend = new DelegateCommand(removeFriend, canRemoveFriend);
            Conversation = new ObservableCollection<MessageNotification>();
            AddPresenceStatuses();
            listener = new Listening();
            DownloadFriendsList();
        }

        public Visibility ChatSwitchMode 
        {                              
            get { return _chatSwitchMode; }
            set
            {
                _chatSwitchMode = value;
                OnPropertyChanged();
                ChangeFormVisibility.RaiseCanExecuteChanged();
            }
        }
        
        public Visibility ChatSwitchMode2
        {
            get { return _chatSwitchMode2; }
            set
            {
                _chatSwitchMode2 = value;
                OnPropertyChanged();
                ChangeFormVisibility.RaiseCanExecuteChanged();
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
                MessageBox.Show(_presenceStatus.ToString());
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
                if (!_messagesDictionary.ContainsKey(Friend.Login))
                    _messagesDictionary.Add(Friend.Login, new ObservableCollection<MessageNotification>());
                Conversation = _messagesDictionary[Friend.Login];
                RemoveFriend.RaiseCanExecuteChanged();
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
        private ICollection<User> _allUsersList = new List<User>();
        private IDictionary<string, ObservableCollection<MessageNotification>> _messagesDictionary = new Dictionary<string, ObservableCollection<MessageNotification>>();
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void removeFriend()
        {
            Friends.Remove(Friends.First(friend => friend.Login.Equals(Friend.Login)));
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            builder.RegisterType<FriendsList>().As<IFriendsList>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<IFriendsList>();
                var deleteFriendReq = new DeleteFriendReq {Login = Const.User.Login, FriendLogin = Friend.Login};
                writer.DeleteFriend(deleteFriendReq);
            }
        }

        private bool canRemoveFriend()
        {
            return Friend != null;
        }

        private void addFriend()
        {
            var user = new User
            {
                Login = FriendLogin,
                Status = _allUsersList.First(u => u.Login.Equals(FriendLogin)).Status
            };
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            builder.RegisterType<FriendsList>().As<IFriendsList>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<IFriendsList>();
                var addFriendReq = new AddFriendReq {Login = Const.User.Login, FriendLogin = FriendLogin};
                writer.AddFriend(addFriendReq);
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
            try
            {
                var activityReq = new ActivityReq
                {
                    Login = Const.User.Login,
                    IsWriting = !string.IsNullOrEmpty(Message),
                    Recipient = Friend.Login
                };
                var builder = new ContainerBuilder();
                builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
                builder.RegisterType<Activity>().As<IActivity>();
                var container = builder.Build();
                using (var scope = container.BeginLifetimeScope())
                {
                    var writer = scope.Resolve<IActivity>();
                    writer.ActivityReq(activityReq);
                }
            }
            catch { }
            return !string.IsNullOrEmpty(Message) && !string.IsNullOrEmpty(Friend.Login);
        }

        private bool canAddFriend()
        {
            return _allUsersList.Any(user => user.Login.Equals(FriendLogin)) && !FriendLogin.Equals(Const.User.Login);
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
            Const.User.Status = PresenceStatus.Offline;
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
            Application.Current.Shutdown();
        }

        private void addEmoticon(string emoticon)
        {
            Message += " " + emoticon + " ";
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
                    _allUsersList.Clear();
                    foreach (var user in response.UserListReqResponse(reqUserList).Users)
                    {
                        _allUsersList.Add(user);
                    }
                }
            }
            catch{}
        }

        private void DownloadFriendsList()
        {
            //try
            {
                var builder = new ContainerBuilder();
                builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
                builder.RegisterType<UsersList>().As<IUsersList>();
                var container = builder.Build();
                using (var scope = container.BeginLifetimeScope())
                {
                    var reqUserList = new UserListReq { Login = Const.User.Login };
                    var response = scope.Resolve<IUsersList>();
                    _allUsersList.Clear();
                    foreach (var user in response.GetFriendsListWithPresenceStatus(reqUserList).Users)
                    {
                        Friends.Add(user);
                    }
                }
            }
            //catch { }
        }
    }

    public class PresenceStatusView
    {
        public PresenceStatus PresenceStatus { get; set; }
        public string Value { get; set; }
    }
}
