using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
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
        public DelegateCommand ChangeFormVisibility { get; private set; }
        public DelegateCommand ViewEmoticons { get; private set; }
        public DelegateCommand AddAttachment { get; private set; }
        public DelegateCommand Close { get; private set; }
        public DelegateCommand<string> AddEmoticon { get; private set; }
        public DelegateCommand FontVisibility { get; private set; }
        public DelegateCommand AddFriend { get; private set; }
        public DelegateCommand SendMessage { get; private set; }
        public DelegateCommand RemoveFriend { get; private set; }
        public DelegateCommand DeleteAttachment { get; private set; }
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
            DeleteAttachment = new DelegateCommand(deleteAttachment, canDeleteAttachment);
            Conversation = new ObservableCollection<MessageNotification>();
            AddPresenceStatuses();      
            DownloadFriendsList();
            IsWriting = Visibility.Collapsed;
            
            th = new Thread(doListen) {IsBackground = true};
            th.Start();
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

        public string PresenceStatus
        {
            get { return _presenceStatus; }
            set
            {
                _presenceStatus = value;
                OnPropertyChanged();
                MessageBox.Show(_presenceStatus);
                if (_presenceStatus.Equals("Online"))
                    Const.User.Status = Common.PresenceStatus.Online;
                if (_presenceStatus.Equals("Away from keyboard"))
                    Const.User.Status = Common.PresenceStatus.Afk;
                if (_presenceStatus.Equals("Offline"))
                    Const.User.Status = Common.PresenceStatus.Offline;
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
                if (!string.IsNullOrEmpty(FriendLogin))
                {
                    if (!_messagesDictionary.ContainsKey(FriendLogin))
                        _messagesDictionary.Add(FriendLogin, new ObservableCollection<MessageNotification>());
                }
                if (Friend != null)
                {
                    if (!_messagesDictionary.ContainsKey(Friend.Login))
                        _messagesDictionary.Add(Friend.Login, new ObservableCollection<MessageNotification>());
                    Conversation = _messagesDictionary[Friend.Login];
                }
                RemoveFriend.RaiseCanExecuteChanged();
            }
        }

        public Visibility IsWriting
        {
            get { return _isWriting; }
            set
            {
                _isWriting = value;
                OnPropertyChanged();
            }
        }

        public string WritingUser
        {
            get { return _writingUser; }
            set
            {
                _writingUser = value;
                OnPropertyChanged();
            }
        }

        public string AttachmentName
        {
            get { return _attachment.Name; }
            set
            {
                _attachment.Name = value;
                OnPropertyChanged();
                DeleteAttachment.RaiseCanExecuteChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility _chatSwitchMode = Visibility.Visible;
        private Visibility _emoticonsVisibility = Visibility.Collapsed;
        private Visibility _font = Visibility.Collapsed;
        private Visibility _chatSwitchMode2 = Visibility.Collapsed, _isWriting = Visibility.Collapsed;
        private string _presenceStatus = "Online";
        private string _friendLogin;
        private string _message, _writingUser;
        private User _friend;
        private Attachment _attachment = new Attachment();
        private ICollection<User> _allUsersList = new List<User>();
        private IDictionary<string, ObservableCollection<MessageNotification>> _messagesDictionary = new Dictionary<string, ObservableCollection<MessageNotification>>();
        private Thread th;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void removeFriend()
        {
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
            Friends.Remove(Friends.First(friend => friend.Login.Equals(Friend.Login)));
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
                    SendTime = new DateTimeOffset().LocalDateTime,
                    Attachment = _attachment,
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

                Conversation.Add(new MessageNotification() { 
                    Attachment = message.Attachment, SendTime = message.SendTime,
                    Message = message.Message, Recipient = message.Recipient, Sender = message.Login
                });
            }
            catch { }

        }

        private bool canSendMessage()
        {
            if (Friend == null) return false;
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
            var click = window.ShowDialog();
            if (click != true) return;
            if (new FileInfo(window.FileName).Length > 1048576)
            {
                MessageBox.Show("File is too big( > 1 MB)");
                return;
            }
            AttachmentName = window.SafeFileName;
            _attachment.Name = window.SafeFileName;
            _attachment.MimeType = MimeMapping.GetMimeMapping(window.SafeFileName);
            _attachment.Data = Encoding.UTF8.GetBytes(Convert.ToBase64String(File.ReadAllBytes(window.FileName)));
        }

        private void deleteAttachment()
        {
            _attachment.Name = null;
            _attachment.MimeType = null;
            _attachment.Data = null;
            AttachmentName = null;
        }

        private bool canDeleteAttachment()
        {
            return !string.IsNullOrEmpty(AttachmentName);
        }

        private void closeApplication()
        {
            Const.User.Status = Common.PresenceStatus.Offline;
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
            th.Abort();
            Application.Current.Shutdown();
        }

        private void addEmoticon(string emoticon)
        {
            Message += " " + emoticon + " ";
        }

        private void AddPresenceStatuses()
        {
            PresenceStatuses.Add(new PresenceStatusView { PresenceStatus = Common.PresenceStatus.Online, Value = "Online" });
            PresenceStatuses.Add(new PresenceStatusView { PresenceStatus = Common.PresenceStatus.Afk, Value = "Away from keyboard" });
            PresenceStatuses.Add(new PresenceStatusView { PresenceStatus = Common.PresenceStatus.Offline, Value = "Offline" });
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
            try
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
            catch { }
        }

        void doListen()
        {
            var listener = new Listening();
            while (th != null && th.IsAlive)
            {
                var msg = listener.ListeningMessages();
                if (msg != null)
                    Conversation.Add(new MessageNotification()
                    {
                        Message = msg.Message,
                        Attachment = msg.Attachment,
                        Sender = msg.Recipient,
                        Recipient = Const.User.Login,
                        SendTime = msg.SendTime
                    });

                var act = listener.ListeningActivity();
                if (act != null)
                {
                    if (Friend != null)
                    {
                        if (act.IsWriting && Friend.Login.Equals(act.Login))
                        {
                            IsWriting = Visibility.Visible;
                            WritingUser = act.Login;
                        }
                        else
                            IsWriting = Visibility.Collapsed;
                    }
                }

                var prs = listener.ListeningPresenceStatus();
                if (prs != null)
                {
                    MessageBox.Show(prs.Login+ " "+prs.PresenceStatus);
                }
            }
        }

    }

    public class PresenceStatusView
    {
        public PresenceStatus PresenceStatus { get; set; }
        public string Value { get; set; }
    }
}
