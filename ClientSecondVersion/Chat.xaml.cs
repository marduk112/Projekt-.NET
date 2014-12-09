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
using System.Windows.Shapes;
using System.Threading;
using Client.Properties;

namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public Chat()
        {
            InitializeComponent();
            this.rtxtDialogueWindow.Document.Blocks.Clear();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.elEmoticons.Visibility = System.Windows.Visibility.Visible;
            this.reEmoticons.Visibility = System.Windows.Visibility.Visible;
            this.btnClose2.Visibility = System.Windows.Visibility.Visible;
            this.imAngel.Visibility = System.Windows.Visibility.Visible;
            this.imAshamed.Visibility = System.Windows.Visibility.Visible;
            this.imCry.Visibility = System.Windows.Visibility.Visible;
            this.imHeart.Visibility = System.Windows.Visibility.Visible;
            this.imKiss.Visibility = System.Windows.Visibility.Visible;
            this.imMad.Visibility = System.Windows.Visibility.Visible;
            this.imScared.Visibility = System.Windows.Visibility.Visible;
            this.imSmile.Visibility = System.Windows.Visibility.Visible;
            this.imSurprised.Visibility = System.Windows.Visibility.Visible;
            this.imTongue.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnTextSettings_Click(object sender, RoutedEventArgs e)
        {
            this.elFont.Visibility = System.Windows.Visibility.Visible;
            this.reFont.Visibility = System.Windows.Visibility.Visible;
            this.lblFontColor.Visibility = System.Windows.Visibility.Visible;
            this.lblFontSize.Visibility = System.Windows.Visibility.Visible;
            this.lblFontType.Visibility = System.Windows.Visibility.Visible;
            this.btnClose1.Visibility = System.Windows.Visibility.Visible;
            this.chbItalic.Visibility = System.Windows.Visibility.Visible;
            this.chbBold.Visibility = System.Windows.Visibility.Visible;
            this.cmbbFontColor.Visibility = System.Windows.Visibility.Visible;
            this.cmbbFontSize.Visibility = System.Windows.Visibility.Visible;
            this.cmbbFontType.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnAttach_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openfiledialog = new Microsoft.Win32.OpenFileDialog();
            openfiledialog.ShowDialog();
            string filename = openfiledialog.FileName;
            
        }

        private void btnClose_Click2(object sender, RoutedEventArgs e)
        {
            this.elEmoticons.Visibility = System.Windows.Visibility.Hidden;
            this.reEmoticons.Visibility = System.Windows.Visibility.Hidden;
            this.btnClose2.Visibility = System.Windows.Visibility.Hidden;
            this.imAngel.Visibility = System.Windows.Visibility.Hidden;
            this.imAshamed.Visibility = System.Windows.Visibility.Hidden;
            this.imCry.Visibility = System.Windows.Visibility.Hidden;
            this.imHeart.Visibility = System.Windows.Visibility.Hidden;
            this.imKiss.Visibility = System.Windows.Visibility.Hidden;
            this.imMad.Visibility = System.Windows.Visibility.Hidden;
            this.imScared.Visibility = System.Windows.Visibility.Hidden;
            this.imSmile.Visibility = System.Windows.Visibility.Hidden;
            this.imSurprised.Visibility = System.Windows.Visibility.Hidden;
            this.imTongue.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnClose_Click1(object sender, RoutedEventArgs e)
        {
            this.elFont.Visibility = System.Windows.Visibility.Hidden;
            this.reFont.Visibility = System.Windows.Visibility.Hidden;
            this.lblFontColor.Visibility = System.Windows.Visibility.Hidden;
            this.lblFontSize.Visibility = System.Windows.Visibility.Hidden;
            this.lblFontType.Visibility = System.Windows.Visibility.Hidden;
            this.btnClose1.Visibility = System.Windows.Visibility.Hidden;
            this.chbItalic.Visibility = System.Windows.Visibility.Hidden;
            this.chbBold.Visibility = System.Windows.Visibility.Hidden;
            this.cmbbFontColor.Visibility = System.Windows.Visibility.Hidden;
            this.cmbbFontSize.Visibility = System.Windows.Visibility.Hidden;
            this.cmbbFontType.Visibility = System.Windows.Visibility.Hidden;
        }


        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            Paragraph date = new Paragraph(new Run(DateTime.Now.ToString()));
            Paragraph p;
            date.FontWeight = FontWeights.Bold;
            date.TextAlignment = TextAlignment.Right;
            this.rtxtDialogueWindow.Document.Blocks.Add(date);
            this.rtxtDialogueWindow.Document.Blocks.Add(p=new Paragraph(new Run(txtMessageWindow.Text)));
            p.Foreground = this.txtMessageWindow.Foreground;
            p.FontFamily = this.txtMessageWindow.FontFamily;
            p.FontSize = this.txtMessageWindow.FontSize;
            p.FontStyle = this.txtMessageWindow.FontStyle;
            p.FontWeight = this.txtMessageWindow.FontWeight;
            this.txtMessageWindow.Clear();
        }

        private void IfCheckedI(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontStyle = FontStyles.Italic;
        }

        private void IfCheckedB(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontWeight = FontWeights.Bold;
        }

        private void IfUncheckedB(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontWeight = FontWeights.Normal;
        }

        private void IfUncheckedI(object sender, RoutedEventArgs e)
        {
            this.txtMessageWindow.FontStyle = FontStyles.Normal;
        }

        private void ClearDialWindow_Click(object sender, RoutedEventArgs e)
        {
            this.rtxtDialogueWindow.Document.Blocks.Clear();
        }
    }
}