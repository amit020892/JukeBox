using System;
using System.Windows;
using System.Windows.Controls;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryButtonControl.xaml
    /// </summary>
    public partial class LibraryButtonControl : UserControl
    {
        public LibraryButtonControl()
        {
            InitializeComponent();
        }

        private void OpenFilterYearsPopup(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.MainWindow.CurrentLibraryInstnace.OpenFilterYearsPopup(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void ShowFavourites(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.MainWindow.CurrentLibraryInstnace.ShowFavourites(null, null);
            }
            catch (Exception)
            {

            }
        }
    }
}
