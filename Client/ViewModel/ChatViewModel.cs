using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
using Microsoft.Win32;
using RabbitMQ.Client;
using Xceed.Wpf.DataGrid.Views;
using PresenceStatus = Common.PresenceStatus;
using System.Threading;

namespace Client.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged, IDisposable
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
        
        public ChatViewModel()
        {
            _listener = new Listening();
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
            AddPresenceStatuses();      
            DownloadFriendsList();
            IsWriting = Visibility.Collapsed;
            th1 = new Thread(doListenMessages){IsBackground = true};
            //th2 = new Thread(doPresenceStatusListen){IsBackground = true};
            th3 = new Thread(doActivityListen) { IsBackground = true };
            th1.Start();
            //th2.Start();
            th3.Start();
            
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
                if (!string.IsNullOrEmpty(FriendLogin))
                {
                    if (!_messagesDictionary.ContainsKey(FriendLogin))
                        _messagesDictionary.Add(FriendLogin, new FlowDocument());
                }
                if (Friend != null)
                {
                    if (!_messagesDictionary.ContainsKey(Friend.Login))
                        _messagesDictionary.Add(Friend.Login, new FlowDocument());
                    Conversation = _messagesDictionary[Friend.Login];
                }
                OnPropertyChanged();
                RemoveFriend.RaiseCanExecuteChanged();
                SendMessage.RaiseCanExecuteChanged();
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

        public FlowDocument Conversation
        {
            get { return _conversation; }
            set
            {
                _conversation = value;
                OnPropertyChanged();
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
        private FlowDocument _conversation;
        private Attachment _attachment = new Attachment();
        private ICollection<User> _allUsersList = new List<User>();
        private SynchronizationContext _ctx = SynchronizationContext.Current;
        private IDictionary<string, FlowDocument> _messagesDictionary = new Dictionary<string, FlowDocument>();
        private Thread th1, th2, th3;
        private readonly Listening _listener;
        private bool _disposed = false;
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
                    SendTime = new DateTimeOffset(DateTime.Now),
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
                var paragraph1 = new Paragraph(new Run(message.SendTime.ToString()))
                {
                    TextAlignment = TextAlignment.Right,
                    FontWeight = FontWeights.Bold
                };
                var paragraph2 = new Paragraph(new Run(message.Login + " wrote\n")) {FontWeight = FontWeights.Bold};
                var paragraph3 = new Paragraph(new Run(message.Message)) { FontSize = paragraph1.FontSize + 3};

                Conversation.Blocks.Add(paragraph1);
                Conversation.Blocks.Add(paragraph2);
                Conversation.Blocks.Add(paragraph3);
                Message = null;
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

        void doListenMessages()
        {
            while (th1 != null && th1.IsAlive)
            {
                var msg = _listener.ListeningMessages();
                if (msg == null) return;
                //Task.Factory.StartNew(thread =>
                //{
                    _ctx.Post(_ =>
                    {
                        try
                        {
                            var paragraph1 = new Paragraph(new Run(msg.SendTime.ToString()))
                            {
                                TextAlignment = TextAlignment.Right,
                                FontWeight = FontWeights.Bold
                            };
                            var paragraph2 = new Paragraph(new Run(msg.Login + " wrote\n"))
                            {
                                FontWeight = FontWeights.Bold
                            };
                            var paragraph3 = new Paragraph(new Run(msg.Message))
                            {
                                FontSize = paragraph1.FontSize + 3
                            };

                            if (!_messagesDictionary.ContainsKey(msg.Login))
                                _messagesDictionary.Add(msg.Login, new FlowDocument());
                            if (!Friends.Any(user => user.Login.Equals(msg.Login)))
                                Friends.Add(new User
                                {
                                    Login = msg.Login,
                                    Status = _allUsersList.First(user => user.Login.Equals(msg.Login)).Status
                                });
                            if (Friend == null)
                            {
                                Friend = Friends.First(user => user.Login.Equals(msg.Login));
                            }
                            else if (!Friend.Login.Equals(msg.Login))
                            {
                                Friend = Friends.First(user => user.Login.Equals(msg.Login));
                            }
                            Conversation = _messagesDictionary[msg.Login];
                            Conversation.Blocks.Add(paragraph1);
                            Conversation.Blocks.Add(paragraph2);
                            Conversation.Blocks.Add(paragraph3);
                            if (string.IsNullOrEmpty(msg.Attachment.Name)) return;
                            var data = Convert.FromBase64String(Encoding.UTF8.GetString(msg.Attachment.Data));
                            var window = new SaveFileDialog
                            {
                                Title = "Save Attachment",
                                FileName = msg.Attachment.Name,
                                Filter =
                                    @"(*" + Path.GetExtension(msg.Attachment.Name) + ")|(*." +
                                    Path.GetExtension(msg.Attachment.Name) + ")"
                            };
                            var t = window.ShowDialog();
                            if (t == true)
                                File.WriteAllBytes(window.FileName, data);
                        }
                        catch
                        {
                        }
                    }, null);
               // }, null);
            }
        }

        void doPresenceStatusListen()
        {
            while (th2 != null & th2.IsAlive)
            {
                var prs = _listener.ListeningPresenceStatus();
                if (prs == null) return;
                try
                {
                    /*Task.Factory.StartNew(_ =>
                    {
                        //MessageBox.Show(prs.Login + " " + prs.PresenceStatus);
                    }, null);*/
                }
                catch { }
            }
        }

        void doActivityListen()
        {
            while (th3 != null & th3.IsAlive)
            {
                var act = _listener.ListeningActivity();
                if (act == null) return;
                //Task.Factory.StartNew(_ =>
                   // {
                        try
                        {

                            if (Friend == null) return;
                            if (act.IsWriting && Friend.Login.Equals(act.Login))
                            {
                                IsWriting = Visibility.Visible;
                                WritingUser = act.Login;
                            }
                            else
                                IsWriting = Visibility.Collapsed;
                        }
                        catch { }
                   // }, null);
            }
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Free other state (managed objects).
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            _disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        ~ChatViewModel()
        {
            // Simply call Dispose(false).
            Dispose (false);
        }
    }

    public class PresenceStatusView
    {
        public PresenceStatus PresenceStatus { get; set; }
        public string Value { get; set; }
    }
}
