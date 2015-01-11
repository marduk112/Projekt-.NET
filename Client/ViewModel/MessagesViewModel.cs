using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Client.Annotations;
using Common;
using Microsoft.Practices.Prism.Commands;

namespace Client.ViewModel
{
    //to determube
    public sealed class MessagesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<MessageNotification> Messages { get; set; }
        public DelegateCommand AddMessage { get; private set; }

        public MessagesViewModel()
        {
            Messages = new ObservableCollection<MessageNotification>();
            AddMessage = new DelegateCommand(Add);
        }

        public string Sender
        {
            get { return _sender; }
            set
            {
                _sender = value;
                OnPropertyChanged();
                AddMessage.RaiseCanExecuteChanged();
            }
        }
        public string Recipient
        {
            get { return _recipient; }
            set
            {
                _recipient = value;
                OnPropertyChanged();
                AddMessage.RaiseCanExecuteChanged();
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
                AddMessage.RaiseCanExecuteChanged();
            }
        }
        public DateTimeOffset SendTime
        {
            get { return _sendTime; }
            set
            {
                _sendTime = value;
                OnPropertyChanged();
                AddMessage.RaiseCanExecuteChanged();
            }
        }
        public Attachment Attachment
        {
            get { return _attachment; }
            set
            {
                _attachment = value;
                OnPropertyChanged();
                AddMessage.RaiseCanExecuteChanged();
            }
        }
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Add()
        {
            var message = new MessageNotification
            {
                Attachment = Attachment,
                Message = Message,
                Recipient = Recipient,
                SendTime = SendTime,
                Sender = Sender
            };
            Messages.Add(message);
        }

        private string _sender;
        private string _recipient;
        private string _message;
        private DateTimeOffset _sendTime;
        private Attachment _attachment;
    }

    /*internal class Comparator : IComparer<MessageNotification>
    {
        public int Compare(MessageNotification x, MessageNotification y)
        {
            return x.SendTime.CompareTo(y.SendTime);
        }
    }*/
}
