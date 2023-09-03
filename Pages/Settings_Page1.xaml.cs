using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for Settings_Page1.xaml
    /// </summary>
    public partial class Settings_Page1 : Page
    {
        public int VolumeControlIncrement
        {
            get
            {
                return Global.AdminSettings.VolumeIncrement;
            }
            set
            {
                Global.AdminSettings.VolumeIncrement = value;
                BtnVolumn.Content = value + "%";
            }
        }


        public Settings_Page1()
        {
            InitializeComponent();
            Global.AdminSettings.IsMusicLibraryScrollHorizontal = true;
            if (Global.AdminSettings.ShowKeyboard)
            {
                btnToggleOnScreenKeyboardOn.IsChecked = true;
                btnToggleOnScreenKeyboardOff.IsChecked = false;
            }
            else
            {
                btnToggleOnScreenKeyboardOn.IsChecked = false;
                btnToggleOnScreenKeyboardOff.IsChecked = true;
            }
            if (Global.AdminSettings.isTextButtonOverlay)
            {
                BtnTextOverlayOn.IsChecked = true;
                BtnTextOverLayOff.IsChecked = false;
            }
            else
            {
                BtnTextOverlayOn.IsChecked = false;
                BtnTextOverLayOff.IsChecked = true;
            }

            BtnVolumn.Content = Global.AdminSettings.VolumeIncrement + "%";

            switch (Global.AdminSettings.AudioVisualizerSize)
            {
                case Global.ClassAdminSettings.Quality.Off:
                    AudioVisualizerQualityOff_Click(null, null);
                    break;
                case Global.ClassAdminSettings.Quality.Low:
                    AudioVisualizerQualityLow_Click(null, null);
                    break;
                case Global.ClassAdminSettings.Quality.Medium:
                    AudioVisualizerQualityMedium_Click(null, null);
                    break;
                case Global.ClassAdminSettings.Quality.High:
                    AudioVisualizerQualityHigh_Click(null, null);
                    break;

            }

            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                MusicLibraryScrollHorz.IsChecked = true;
                MusicLibraryScrollVert.IsChecked = false;
            }
            else
            {
                MusicLibraryScrollVert.IsChecked = true;
                MusicLibraryScrollHorz.IsChecked = false;
            }
        }

        private void btnTextButtonOverlayON_Click(object sender, RoutedEventArgs e)
        {
            if (!BtnTextOverlayOn.IsChecked.Value)
            {
                BtnTextOverlayOn.IsChecked = true;
            }

            BtnTextOverLayOff.IsChecked = false;
            Global.AdminSettings.isTextButtonOverlay = true;
        }
        private void btnTextButtonOverlayOFF_Click(object sender, RoutedEventArgs e)
        {
            if (!BtnTextOverLayOff.IsChecked.Value)
            {
                BtnTextOverLayOff.IsChecked = true;
            }

            BtnTextOverlayOn.IsChecked = false;
            Global.AdminSettings.isTextButtonOverlay = false;
        }
        private void btnVolumeUp_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeControlIncrement <= 200)
                VolumeControlIncrement += 40;
            else
                VolumeControlIncrement = 200;
            Global.AdminSettings.VolumeIncrement = VolumeControlIncrement;
            if (Global.MainWindow?.VolControlPanel != null)
            {
                try
                {
                    if (Global.AdminSettings.VolumeIncrement == 40)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel1_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 80)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel2_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 120)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel3_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 160)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel4_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 200)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel5_Click(null, null);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void btnVolumeDown_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeControlIncrement >= 40)
                VolumeControlIncrement -= 40;
            Global.AdminSettings.VolumeIncrement = VolumeControlIncrement;
            if (Global.MainWindow?.VolControlPanel != null)
            {
                try
                {
                    if (Global.AdminSettings.VolumeIncrement == 40)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel1A_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 80)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel2A_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 120)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel3A_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 160)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel4A_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 200)
                    {
                        Global.MainWindow?.VolControlPanel.volLevel5A_Click(null, null);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void btnChangeTheme_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnToggleOnScreenKeyboardOn_click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.ShowKeyboard = true;
            btnToggleOnScreenKeyboardOn.IsChecked = true;
            btnToggleOnScreenKeyboardOff.IsChecked = false;
        }
        private void btnToggleOnScreenKeyboardOff_click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.ShowKeyboard = false;
            btnToggleOnScreenKeyboardOn.IsChecked = false;
            btnToggleOnScreenKeyboardOff.IsChecked = true;
        }

        private void BtnKeyboardSize50_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.KeyboardButtonSize = 50;
            BtnKeyboardSize50.IsChecked = true;
            BtnKeyboardSize75.IsChecked = false;
        }
        private void BtnKeyboardSize75_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.KeyboardButtonSize = 75;
            BtnKeyboardSize50.IsChecked = false;
            BtnKeyboardSize75.IsChecked = true;
        }

        private void BtnToggleAlbumNumberOn_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.ShowAlbumNumbers = true;
            BtnToggleAlbumNumberOn.IsChecked = true;
            BtnToggleAlbumNumberOff.IsChecked = false;
        }
        private void BtnToggleAlbumNumberOff_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.ShowAlbumNumbers = false;
            BtnToggleAlbumNumberOn.IsChecked = false;
            BtnToggleAlbumNumberOff.IsChecked = true;
        }

        private void AudioVisualizerQualityOff_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.AudioVisualizerSize = Global.ClassAdminSettings.Quality.Off;
            AudioVisualizerQualityOff.IsChecked = true;
            AudioVisualizerQualityLow.IsChecked = false;
            AudioVisualizerQualityMedium.IsChecked = false;
            AudioVisualizerQualityHigh.IsChecked = false;
            if (Global.mPlayer.SongPlayingArtWork != null)
            {
                Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Visible;
            }
        }
        private void AudioVisualizerQualityLow_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.AudioVisualizerSize = Global.ClassAdminSettings.Quality.Low;
            AudioVisualizerQualityOff.IsChecked = false;
            AudioVisualizerQualityLow.IsChecked = true;
            AudioVisualizerQualityMedium.IsChecked = false;
            AudioVisualizerQualityHigh.IsChecked = false;
            if (Global.mPlayer.SongPlayingArtWork != null)
            {
                Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
            }
        }
        private void AudioVisualizerQualityMedium_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.AudioVisualizerSize = Global.ClassAdminSettings.Quality.Medium;
            AudioVisualizerQualityOff.IsChecked = false;
            AudioVisualizerQualityLow.IsChecked = false;
            AudioVisualizerQualityMedium.IsChecked = true;
            AudioVisualizerQualityHigh.IsChecked = false;
            if (Global.mPlayer.SongPlayingArtWork != null)
            {
                Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
            }
        }
        private void AudioVisualizerQualityHigh_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.AudioVisualizerSize = Global.ClassAdminSettings.Quality.High;
            AudioVisualizerQualityOff.IsChecked = false;
            AudioVisualizerQualityLow.IsChecked = false;
            AudioVisualizerQualityMedium.IsChecked = false;
            AudioVisualizerQualityHigh.IsChecked = true;
            if (Global.mPlayer.SongPlayingArtWork != null)
            {
                Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
            }
        }

        private void MusicLibraryScrollHorz_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.IsMusicLibraryScrollHorizontal=true;
            MusicLibraryScrollHorz.IsChecked = true;
            MusicLibraryScrollVert.IsChecked = false;
            Global.MainWindow.KaraokeLibraryInstancesLoaded = false;
            Global.MainWindow.MusicLibraryInstancesLoaded = false;
            Global.MainWindow.RadioLibraryInstancesLoaded = false;
            Global.MainWindow.VideoLibraryInstancesLoaded = false;
        }
        private void MusicLibraryScrollVert_Click(object sender, RoutedEventArgs e)
        {
            Global.AdminSettings.IsMusicLibraryScrollHorizontal = false;
            MusicLibraryScrollHorz.IsChecked = false;
            MusicLibraryScrollVert.IsChecked = true;
            Global.MainWindow.KaraokeLibraryInstancesLoaded = false;
            Global.MainWindow.MusicLibraryInstancesLoaded = false;
            Global.MainWindow.RadioLibraryInstancesLoaded = false;
            Global.MainWindow.VideoLibraryInstancesLoaded = false;
        }
    }
}
