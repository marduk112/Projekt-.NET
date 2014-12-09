using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Client.Modules;
using Client.Notifies;
using Common;
using PresenceStatus = Common.PresenceStatus;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _loginWindow.Show();
            _loginWindow.Closing += loginWindow_Closing;
            _user.Status = PresenceStatus.Offline;
        }

        private LoginWindow _loginWindow = new LoginWindow();
        private readonly User _user = new User();
        private FriendsCollection _friendsCollection = new FriendsCollection();
        private MessagesCollection _messagesCollection = new MessagesCollection();
        private List<Thread> threadsList = new List<Thread>();
        private void loginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_loginWindow.AuthResponse.Status != Status.OK) return;
            this.IsEnabled = true;
            LoginOutButton.Content = "Logout";
            _user.Login = _loginWindow.Login;
            _user.Status = PresenceStatus.Online;
            _loginWindow = null;
            var thread = new Thread(() =>
            {
                var activity = new Activity(_user.Login);
                while (true)
                {
                    var activityResponse = activity.ActivityResponse();
                    //add info; activity status
                }
            }) {IsBackground = true};
            threadsList.Add(thread);
            thread.Start();
        }
        private void LoginOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginOutButton.Content.Equals("Login"))
                _loginWindow.Show();
            //must add logout
            if (!LoginOutButton.Content.Equals("Logout")) return;
            LoginOutButton.Content = "Login";
            this.IsEnabled = false;
            _loginWindow = new LoginWindow();
            _loginWindow.Show();
        }
        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _loginWindow.Close();
        }
    }
}
