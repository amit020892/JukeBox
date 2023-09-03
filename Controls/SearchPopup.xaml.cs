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

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for SearchPopup.xaml
    /// </summary>
    public partial class SearchPopup : UserControl
    {
        internal Library.SearchMode bufferSearchMode = Library.SearchMode.All;
        public SearchPopup()
        {
            InitializeComponent();
            searchText.Text = "Enter your search";
            searchText.GotFocus += RemovePlaceholder;
            searchText.LostFocus += AddPlaceholder;
            AllBtn.Background = Brushes.White;
            AllBtn.Foreground = Brushes.Black;
            ArtistBtn.Background = Brushes.Transparent;
            ArtistBtn.Foreground = Brushes.White;
            AlbumBtn.Background = Brushes.Transparent;
            AlbumBtn.Foreground = Brushes.White;
            SongBtn.Background = Brushes.Transparent;
            SongBtn.Foreground = Brushes.White;
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(searchText.Text))
            {
                searchText.Text = "Enter your search";
            }
        }

        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if(searchText.Text== "Enter your search")
            {
                searchText.Text = string.Empty;
            }
        }

        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
            Global.mPlayer.popUp_Frame.Content = null;
            Global.MainWindow.btnLogo.IsEnabled = true;
        }

        private void AllBtn_Click(object sender, RoutedEventArgs e)
        {
            bufferSearchMode=Library.SearchMode.All;
            AllBtn.Background = Brushes.White;
            AllBtn.Foreground = (Brush)(new BrushConverter().ConvertFrom("#225663")); ;
            ArtistBtn.Background = Brushes.Transparent;
            ArtistBtn.Foreground = Brushes.White;
            AlbumBtn.Background = Brushes.Transparent;
            AlbumBtn.Foreground = Brushes.White; 
            SongBtn.Background = Brushes.Transparent;
            SongBtn.Foreground = Brushes.White;
        }
        private void ArtistBtn_Click(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.Artist;
            AllBtn.Background = Brushes.Transparent;
            AllBtn.Foreground = Brushes.White;
            ArtistBtn.Background = Brushes.White;
            ArtistBtn.Foreground = (Brush)(new BrushConverter().ConvertFrom("#225663")); 
            AlbumBtn.Background = Brushes.Transparent;
            AlbumBtn.Foreground = Brushes.White; 
            SongBtn.Background = Brushes.Transparent;
            SongBtn.Foreground = Brushes.White;
        }
        private void AlbumBtn_Click(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.Album;
            AllBtn.Background = Brushes.Transparent;
            AllBtn.Foreground = Brushes.White;
            ArtistBtn.Background = Brushes.Transparent;
            ArtistBtn.Foreground = Brushes.White;
            AlbumBtn.Background = Brushes.White;
            AlbumBtn.Foreground = (Brush)(new BrushConverter().ConvertFrom("#225663"));
            SongBtn.Background = Brushes.Transparent;
            SongBtn.Foreground = Brushes.White;
        }
        private void SongBtn_Click(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.Song;
            AllBtn.Background = Brushes.Transparent;
            AllBtn.Foreground = Brushes.White;
            ArtistBtn.Background = Brushes.Transparent;
            ArtistBtn.Foreground = Brushes.White;
            AlbumBtn.Background = Brushes.Transparent;
            AlbumBtn.Foreground = Brushes.White; 
            SongBtn.Background = Brushes.White;
            SongBtn.Foreground = (Brush)(new BrushConverter().ConvertFrom("#225663"));
        }

        private void numberBtn_click(object sender, RoutedEventArgs e)
        {
            NumKeyboard.Visibility = Visibility.Visible;
            AlphaKeyboard.Visibility = Visibility.Collapsed;
        }

        private void clearBtn_click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchText.Text =string.Empty;
                searchText.Text = "Enter your search";
            }
            catch (Exception)
            {

            }
        }

        private void spaceBtn_click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchText.Text = searchText.Text + " ";
            }
            catch (Exception)
            {

            }
        }

        private void txtBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (searchText.Text == "Enter your search")
                {
                    searchText.Text = string.Empty;
                    searchText.Foreground = Brushes.Black;
                }
                if (!IsUppercaseBtnChecked)
                {
                    searchText.Text = searchText.Text + (sender as Button).Content;
                }
                else
                {
                    searchText.Text = searchText.Text + (sender as Button).Content.ToString().ToUpper();
                }
               
            }
            catch (Exception)
            {

            }
        }

        private void backSpaceBtn_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(searchText.Text.Length > 0 && searchText.Text != "Enter your search")
                {
                    searchText.Text= searchText.Text.Substring(0, searchText.Text.Length - 1);
                }
            }
            catch (Exception)
            {

            }            
        }

        private bool IsUppercaseBtnChecked { get; set; }
        private void upperspaceBtn_click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsUppercaseBtnChecked = !IsUppercaseBtnChecked;
                foreach (var item in KeyboardRowStack1.Children)
                {
                    (item as Button).Content = IsUppercaseBtnChecked ? (item as Button)?.Content.ToString().ToUpper() : (item as Button)?.Content.ToString().ToLower();
                }
                foreach (var item in KeyboardRowStack2.Children)
                {
                    (item as Button).Content = IsUppercaseBtnChecked ? (item as Button)?.Content.ToString().ToUpper() : (item as Button)?.Content.ToString().ToLower();
                }
                foreach (var item in KeyboardRowStack3.Children)
                {
                    (item as Button).Content = IsUppercaseBtnChecked ? (item as Button)?.Content.ToString().ToUpper() : (item as Button)?.Content.ToString().ToLower();
                }
            }
            catch (Exception)
            {

            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Library)Global.MainFrame.Content).SearchbySearchFilter(searchText.Text, bufferSearchMode);
            }
            catch (Exception)
            {
            }
            
        }

        private void AlphaBtn_click(object sender, RoutedEventArgs e)
        {
            NumKeyboard.Visibility = Visibility.Collapsed;
            AlphaKeyboard.Visibility = Visibility.Visible;
        }
    }
}
