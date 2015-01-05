using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Client.Annotations;
using Common;
using Microsoft.Practices.Prism.Commands;

namespace Client.Notifies
{
    public sealed class FriendsCollection : INotifyPropertyChanged
    {
        public FriendsCollection()
        {
            Friends = new ObservableCollection<User>();
            AddFriend = new DelegateCommand(Add);
            DeleteFriend = new DelegateCommand(Delete);
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

        public DelegateCommand AddFriend { get; private set; }
        public DelegateCommand DeleteFriend { get; private set; }
        public ObservableCollection<User> Friends { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
       
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Add()
        {
            var friend = new User {Login = Login, Status = Status};
            foreach (var item in Friends.Where(item => item.Login.Equals(Login)))
            {
                Friends.Remove(item);
                break;
            }
            Friends.Add(friend);
        }

        private void Delete()
        {
            var friend = new User { Login = Login, Status = Status };
            Friends.Remove(friend);
        }

        private string _login;
        private PresenceStatus _status;

        
        
    }
}
