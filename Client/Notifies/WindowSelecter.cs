using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using Client.Annotations;
using System.Runtime.CompilerServices;

namespace Client.Notifies
{
    public class WindowSelecter : INotifyPropertyChanged  
    {
        public DelegateCommand SelectForm { get; private set; }

        public WindowSelecter()
        {
            SelectForm = new DelegateCommand(doSelectForm);
        }
        
        private Visibility _chatSwitchMode = Visibility.Visible;

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

        private void doSelectForm() 
        {
            switch (ChatSwitchMode)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    ChatSwitchMode = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    ChatSwitchMode = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
