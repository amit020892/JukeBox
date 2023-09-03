using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for VolumnControlPanel.xaml
    /// </summary>
    public partial class VolumnControlPanel : UserControl
    {
        int MinVolume = 40;
        int ActiveCount = 1;
        public VolumnControlPanel()
        {
            InitializeComponent();
            try
            {
                if (Global.AdminSettings.VolumeIncrement == 40)
                {
                    volLevel1_Click(null, null);
                }
                else if (Global.AdminSettings.VolumeIncrement == 80)
                {
                    volLevel2_Click(null, null);
                }
                else if (Global.AdminSettings.VolumeIncrement == 120)
                {
                    volLevel3_Click(null, null);
                }
            }
            catch (Exception)
            {

            }

        }

        private bool IsBusy = false;
        public void BtnMute_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (!Global.mPlayer.vlcController.SourceProvider.MediaPlayer.Audio.IsMute)
                {
                    Global.mPlayer.vlcController.SourceProvider.MediaPlayer.Audio.ToggleMute();
                    Global.mPlayer.vlcController.SourceProvider.MediaPlayer.OnMediaPlayerMuted();                    
                    btnMuteActive.Visibility = Visibility.Visible;
                    btnMute.Visibility = Visibility.Collapsed;
                    volLevel1.Visibility = Visibility.Visible;
                    volLevel1A.Visibility = Visibility.Collapsed;
                    volLevel2.Visibility = Visibility.Visible;
                    volLevel2A.Visibility = Visibility.Collapsed;
                    volLevel3.Visibility = Visibility.Visible;
                    volLevel3A.Visibility = Visibility.Collapsed;
                    volLevel4.Visibility = Visibility.Visible;
                    volLevel4A.Visibility = Visibility.Collapsed;
                    volLevel5.Visibility = Visibility.Visible;
                    volLevel5A.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                IsBusy = false;
            }


        }

        private void MediaPlayer_Muted(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception)
            {

            }
        }

        public void BtnMuteActive_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (Global.mPlayer.vlcController.SourceProvider.MediaPlayer.Audio.IsMute)
                {
                    Global.mPlayer.vlcController.SourceProvider.MediaPlayer.Audio.ToggleMute();
                    Global.mPlayer.vlcController.SourceProvider.MediaPlayer.OnMediaPlayerUnmuted();
                    volLevel1.Visibility = Visibility.Collapsed;
                    volLevel1A.Visibility = Visibility.Visible;
                    btnMuteActive.Visibility = Visibility.Collapsed;
                    btnMute.Visibility = Visibility.Visible;
                    switch (ActiveCount)
                    {
                        case 1:
                            volLevel1_Click(null, null);
                            break;
                        case 2:
                            volLevel2_Click(null, null);
                            break;
                        case 3:
                            volLevel3_Click(null, null);
                            break;
                        case 4:
                            volLevel4_Click(null, null);
                            break;
                        case 5:
                            volLevel5_Click(null, null);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                IsBusy = false;
            }       
            
        }

        int MaxButtonCount = 5;
        public void volLevel1_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel1.Visibility = Visibility.Collapsed;
            volLevel1A.Visibility = Visibility.Visible;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume));
        }

        public void volLevel1A_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel2.Visibility = Visibility.Visible;
            volLevel2A.Visibility = Visibility.Collapsed;
            volLevel3.Visibility = Visibility.Visible;
            volLevel3A.Visibility = Visibility.Collapsed;
            volLevel4.Visibility = Visibility.Visible;
            volLevel4A.Visibility = Visibility.Collapsed;
            volLevel5.Visibility = Visibility.Visible;
            volLevel5A.Visibility = Visibility.Collapsed;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume));
        }
        public void volLevel2_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel1.Visibility = Visibility.Collapsed;
            volLevel1A.Visibility = Visibility.Visible;
            volLevel2.Visibility = Visibility.Collapsed;
            volLevel2A.Visibility = Visibility.Visible;
            ActiveCount = 2;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 2));
        }

        public void volLevel2A_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel3.Visibility = Visibility.Visible;
            volLevel3A.Visibility = Visibility.Collapsed;
            volLevel4.Visibility = Visibility.Visible;
            volLevel4A.Visibility = Visibility.Collapsed;
            volLevel5.Visibility = Visibility.Visible;
            volLevel5A.Visibility = Visibility.Collapsed;
            ActiveCount = 2;
            Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 2));
        }
        public void volLevel3_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel1.Visibility = Visibility.Collapsed;
            volLevel1A.Visibility = Visibility.Visible;
            volLevel2.Visibility = Visibility.Collapsed;
            volLevel2A.Visibility = Visibility.Visible;
            volLevel3.Visibility = Visibility.Collapsed;
            volLevel3A.Visibility = Visibility.Visible;
            ActiveCount = 3;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 3));
        }

        public void volLevel3A_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel4.Visibility = Visibility.Visible;
            volLevel4A.Visibility = Visibility.Collapsed;
            volLevel5.Visibility = Visibility.Visible;
            volLevel5A.Visibility = Visibility.Collapsed;
            ActiveCount = 3;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 3));
        }
        public void volLevel4_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel1.Visibility = Visibility.Collapsed;
            volLevel1A.Visibility = Visibility.Visible;
            volLevel2.Visibility = Visibility.Collapsed;
            volLevel2A.Visibility = Visibility.Visible;
            volLevel3.Visibility = Visibility.Collapsed;
            volLevel3A.Visibility = Visibility.Visible;
            volLevel4.Visibility = Visibility.Collapsed;
            volLevel4A.Visibility = Visibility.Visible;
            ActiveCount = 4;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 4));
        }

        public void volLevel4A_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel5.Visibility = Visibility.Visible;
            volLevel5A.Visibility = Visibility.Collapsed;
            ActiveCount = 4;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 4));
        }
        public void volLevel5_Click(object sender, MouseButtonEventArgs e)
        {
            volLevel1.Visibility = Visibility.Collapsed;
            volLevel1A.Visibility = Visibility.Visible;
            volLevel2.Visibility = Visibility.Collapsed;
            volLevel2A.Visibility = Visibility.Visible;
            volLevel3.Visibility = Visibility.Collapsed;
            volLevel3A.Visibility = Visibility.Visible;
            volLevel4.Visibility = Visibility.Collapsed;
            volLevel4A.Visibility = Visibility.Visible;
            volLevel5.Visibility = Visibility.Collapsed;
            volLevel5A.Visibility = Visibility.Visible;
            ActiveCount = 5;
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Convert.ToInt32(MinVolume * 5));
        }

        public void volLevel5A_Click(object sender, MouseButtonEventArgs e)
        {

        }

        public void btnVolUp_Click(object sender, RoutedEventArgs e)
        {
            //btnMuteActive.Visibility = Visibility.Collapsed;
            //btnVolDown.Visibility = Visibility.Visible;

        }

        private void btnVolUp_Click(object sender, MouseButtonEventArgs e)
        {
            if (Global.AdminSettings.VolumeIncrement == 0)
            {
                if (btnMuteActive.Visibility == Visibility.Collapsed)
                {
                    Global.AdminSettings.VolumeIncrement += 40;
                }

                volLevel1_Click(sender, e);
            }
            else if (Global.AdminSettings.VolumeIncrement == 40)
            {
                if (btnMuteActive.Visibility == Visibility.Collapsed)
                {
                    Global.AdminSettings.VolumeIncrement += 40;
                    volLevel2_Click(sender, e);
                }
            }
            else if (Global.AdminSettings.VolumeIncrement == 80)
            {
                if (btnMuteActive.Visibility == Visibility.Collapsed)
                {
                    Global.AdminSettings.VolumeIncrement += 40;
                    volLevel3_Click(sender, e);
                }
            }
            else if (Global.AdminSettings.VolumeIncrement == 120)
            {
                if (btnMuteActive.Visibility == Visibility.Collapsed)
                {
                    Global.AdminSettings.VolumeIncrement += 40;
                    volLevel4_Click(sender, e);
                }
            }
            else if (Global.AdminSettings.VolumeIncrement == 160)
            {
                if (btnMuteActive.Visibility == Visibility.Collapsed)
                {
                    Global.AdminSettings.VolumeIncrement += 40;
                    volLevel5_Click(sender, e);
                }
            }

            if (btnMuteActive.Visibility == Visibility.Visible)
            {
                BtnMuteActive_Click(sender, e);
            }
            if (Global.mPlayer != null)
                Global.mPlayer.VolumeUp(Global.AdminSettings.VolumeIncrement);
        }
        private void btnVolDown_Click(object sender, MouseButtonEventArgs e)
        {
            if (btnMuteActive.Visibility == Visibility.Collapsed)
            {
                ActiveCount--;
                if (Global.AdminSettings.VolumeIncrement == 40)
                {
                    Global.AdminSettings.VolumeIncrement -= 40;
                    BtnMute_Click(sender, e);
                }
                else if (Global.AdminSettings.VolumeIncrement == 80)
                {
                    Global.AdminSettings.VolumeIncrement -= 40;
                    volLevel1A_Click(sender, e);
                }
                else if (Global.AdminSettings.VolumeIncrement == 120)
                {
                    Global.AdminSettings.VolumeIncrement -= 40;
                    volLevel2A_Click(sender, e);
                }
                else if (Global.AdminSettings.VolumeIncrement == 160)
                {
                    Global.AdminSettings.VolumeIncrement -= 40;
                    volLevel3A_Click(sender, e);
                }
                else if (Global.AdminSettings.VolumeIncrement == 200)
                {
                    Global.AdminSettings.VolumeIncrement -= 40;
                    volLevel4A_Click(sender, e);
                }
                if (Global.mPlayer != null)
                    Global.mPlayer.VolumeUp(Global.AdminSettings.VolumeIncrement);
            }

        }
    }
}
