﻿using System;
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
using System.Windows.Shapes;
using Autofac;
using Client.Interfaces;
using Client.Modules;
using Client.Notifies;
using Common;
using RabbitMQ.Client;

namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy Contacts.xaml
    /// </summary>
    public partial class Contacts : Window
    {
        public List<ImageSource> myImageSource;

        public Contacts()
        {
            InitializeComponent();
            myImageSource = new List<ImageSource>();
            ImageSource FirstImageSource = new BitmapImage(new Uri("/Image/okozielone.jpg", UriKind.Relative));
            ImageSource SecondImageSource = new BitmapImage(new Uri("/Image/okofioletowe.jpg", UriKind.Relative));
            ImageSource ThirdImageSource = new BitmapImage(new Uri("/Image/okoniebieskie.jpg", UriKind.Relative));
            ImageSource FourthImageSource = new BitmapImage(new Uri("/Image/okobrazowe.jpg", UriKind.Relative));
            ImageSource FifthImageSource = new BitmapImage(new Uri("/Image/okoczerwone.jpg", UriKind.Relative));
            myImageSource.Add(FirstImageSource);
            myImageSource.Add(SecondImageSource);
            myImageSource.Add(ThirdImageSource);
            myImageSource.Add(FourthImageSource);
            myImageSource.Add(FifthImageSource);

            this.cmbbStatus.SelectedIndex = 4;
            this.imStatus.Source = myImageSource.Last();
            usersList.DataContext = _usersCollection;
            //DownloadUsersList();
        }

        public void FriendsCollection(FriendsCollection friendsCollection)
        {
            _friendsCollection = friendsCollection;
        }

        private FriendsCollection _friendsCollection;
        private readonly FriendsCollection _usersCollection = new FriendsCollection();

        private void DownloadUsersList()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<UsersList>().As<IUsersList>();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var reqUserList = new UserListReq { Login = Const.User.Login };
                var response = scope.Resolve<IUsersList>();
                foreach (var user in response.UserListReqResponse(reqUserList).Users)
                {                   
                    _usersCollection.Friends.Add(user);
                }
            }
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

        private void cmbbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cmbiStatus1.IsSelected.ToString() == "True") imStatus.Source = myImageSource[0];
            else if (this.cmbiStatus2.IsSelected.ToString() == "True") imStatus.Source = myImageSource[1];
            else if (this.cmbiStatus3.IsSelected.ToString() == "True") imStatus.Source = myImageSource[2];
            else if (this.cmbiStatus4.IsSelected.ToString() == "True") imStatus.Source = myImageSource[3];
            else imStatus.Source = myImageSource[4];
        }

        private void btnFindUser_Click(object sender, RoutedEventArgs e)
        {
            var filter = txtFindUsers.Text;
            foreach (var user in _usersCollection.AllUsers.Where(user => user.Login.Contains(filter)))
            {
                _usersCollection.AllUsers.Remove(user);
            }
        }

        private void usersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
