using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Client.Annotations;
using Client.RichTextBoxEmoticons;
using Microsoft.Practices.Prism.Commands;
using Xceed.Wpf.DataGrid.Views;

namespace Client.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged  
    {
        public DelegateCommand SelectForm { get; private set; }
        public DelegateCommand ViewEmoticons { get; private set; }
        public DelegateCommand AddAttachment { get; private set; }
        public DelegateCommand<Window> Close { get; private set; }
        public DelegateCommand<object[]> AddEmoticon { get; private set; }
        public DelegateCommand FontVisibility { get; private set; }

        public ChatViewModel()
        {
            SelectForm = new DelegateCommand(doSelectForm);
            ViewEmoticons = new DelegateCommand(viewEmoticons);
            AddAttachment = new DelegateCommand(addAttachment);
            Close = new DelegateCommand<Window>(closeApplication);
            AddEmoticon = new DelegateCommand<object[]>(addEmoticon);
            FontVisibility = new DelegateCommand(fontVisibility);
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

        public event PropertyChangedEventHandler PropertyChanged;


        private Visibility _chatSwitchMode = Visibility.Visible;
        private Visibility _emoticonsVisibility = Visibility.Collapsed;
        private Visibility _font = Visibility.Collapsed;
        private string _pathToAttachment;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
                Filter = "All files (*.*)|*.*",
                AddExtension = true,
                Multiselect = false
            };
            window.ShowDialog();
            _pathToAttachment = window.FileName;
        }

        private void closeApplication(Window window)
        {
            window.Close();
        }

        private void addEmoticon(object[] values)
        {
            var textDialog = values[0] as TextBox;
            var emoticon = values[1] as string;
            textDialog.Text += " " + emoticon + " ";
        }
    }
}
