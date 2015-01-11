using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Client.Annotations;
using Common;
using Microsoft.Practices.Prism.Commands;

namespace Client.ViewModel
{
    public sealed class FriendsViewModel : INotifyPropertyChanged
    {
        public FriendsViewModel()
        {
            Friends = new ObservableCollection<User>();
            AllUsers = new ObservableCollection<User>();
            UsersList = new ObservableCollection<List<User>>();
            AddFriend = new DelegateCommand(Add);
            DeleteFriend = new DelegateCommand(Delete, CanDelete);
            GetSelectedUsers = new DelegateCommand(GetSelectUsers, CanGetSelectUser);
        }

        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged();
                AddFriend.RaiseCanExecuteChanged();
                DeleteFriend.RaiseCanExecuteChanged();
            }
        }

        public PresenceStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
                AddFriend.RaiseCanExecuteChanged();
                DeleteFriend.RaiseCanExecuteChanged();
            }
        }

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged();
                DeleteFriend.RaiseCanExecuteChanged();
            }
        }

        public List<User> UsersListProperty
        {
            get { return _usersList; }
            set
            {
                _usersList = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand AddFriend { get; private set; }
        public DelegateCommand DeleteFriend { get; private set; }
        public DelegateCommand GetSelectedUsers { get; private set; }
        public ObservableCollection<User> Friends { get; set; }
        public ObservableCollection<User> AllUsers { get; set; }
        public ObservableCollection<List<User>> UsersList { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
       
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GetSelectUsers()
        {
            foreach (var user in _usersList)
                Friends.Add(user);
        }

        private void Add()
        {
            var duplicateFriend = Friends.First(e => e.Login.Equals(_user.Login));
            Friends.Remove(duplicateFriend);
            Friends.Add(_user);
        }

        private void Delete()
        {
            var item = Friends.First(e => e.Login.Equals(_user.Login));
            Friends.Remove(item);
        }

        private bool CanDelete()
        {
            return _user != null;
        }

        private bool CanGetSelectUser()
        {
            return _usersList != null;
        }

        private string _login;
        private PresenceStatus _status;
        private User _user;
        private List<User> _usersList;
    }
}
