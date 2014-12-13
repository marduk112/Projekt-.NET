using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Client.Modules;
using Common;

namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            SetLanguageDictionary();
        }

        private void SetLanguageDictionary()
        {
            var dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "pl":
                    dict.Source = new Uri("..\\Resources\\Resources.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
                case "en":
                    dict.Source = new Uri("..\\Resources\\Resources.EN.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\Resources.xaml", UriKind.Relative);
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Ellipse_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void LblName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnRegistry_Click(object sender, RoutedEventArgs e)
        {
            (new Registration()).Show();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            //(new Contacts()).Show();
            var request = new AuthRequest {Login = txtLogin.Text, Password = pswbPassword.Password};
            var authentication = new Login();
            var response = authentication.LoginAuthRequestResponse(request);
            if (response.Status == Status.OK)
            {
                Const.User.Login = txtLogin.Text;
                Const.User.Status = Common.PresenceStatus.Online;
                var req = new Modules.PresenceStatus();
                req.SendPresenceStatus(Const.User);
                new Chat().Show();
                this.Close();
            }
            else
                MessageBox.Show(response.Message);
        }

        private void btnPolish_Click(object sender, RoutedEventArgs e)
        {
            var cultureInfo = new System.Globalization.CultureInfo("pl");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            SetLanguageDictionary();
        }

        private void btnEnglish_Click(object sender, RoutedEventArgs e)
        {
            var cultureInfo = new System.Globalization.CultureInfo("en");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            SetLanguageDictionary();
        }

    }

}