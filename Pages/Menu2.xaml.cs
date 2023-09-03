using iTunesSearch.Library;
using JukeBoxSolutions.Class;
using JukeBoxSolutions.Controls;
using JukeBoxSolutions.Pages;
using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace JukeBoxSolutions
{
    /// <summary>
    /// Interaction logic for Menu2.xaml
    /// </summary>
    public partial class Menu2 : Page
    {
        Frame mainFrame;
        public Menu2(ref Frame MainFrame)
        {
            InitializeComponent();
            mainFrame = MainFrame;
            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                btnAdminMode.IsChecked = true;
                btnAdminMode.Background = Brushes.White;
                btnAdminMode.Foreground = Brushes.Black;
            }
            else
            {
                btnAdminMode.Background = Brushes.Black;
                btnAdminMode.Foreground = Brushes.White;
            }


            LoadWifi();
        }
        private void btnImportMedia_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = new Pages.ImportModeSelection();
        }

        private void btnAdminMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnAdminMode.IsChecked.Value)
            {
                btnAdminMode.Background = Brushes.White;
                btnAdminMode.Foreground = Brushes.Black;
                Global.AppControlMode = Global.AppControlModeEnum.Admin;
                Global.isAdminOverride = true;
                Global.mPlayer.ShowNotification("Application is now set to Admin Mode");
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                btnResetDatabase.IsEnabled = true;

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    if (db.TrackLibraries.Where(w => w.Type == "Import - Karaoke" || w.Type == "Import - Music" || w.Type == "Import - Video").Any())
                    {
                        btnResumeImport.IsEnabled = true;
                        btnResumeImport.Content = "Resume Import";
                    }
                    else
                    {
                        btnResumeImport.IsEnabled = false;
                        btnResumeImport.Content = "Nothing";
                    }

                    if (db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId).Any())
                        btnResumeUpdate.IsEnabled = true;
                    else
                        btnResumeUpdate.IsEnabled = false;

                }
            }
            else
            {
                btnAdminMode.Background = Brushes.Black;
                btnAdminMode.Foreground = Brushes.White;
                Global.AppControlMode = Global.AppControlModeEnum.Normal;
                Global.mPlayer.ShowNotification("Application is now set back to Normal Mode");
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                btnResetDatabase.IsEnabled = false;
                btnResumeImport.IsEnabled = false;
                btnResumeImport.Content = "Resume Import";
                btnResumeUpdate.IsEnabled = false;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = new Pages.Settings();
        }

        bool isActive = false;
        private void ThemeTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Global.AdminSettings.currentThemeSequence)
                {
                    case 1:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/main_background.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/AdminMenuD1.png"));                        
                        break;
                    case 2:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/AdminMenuD2.png"));
                        break;
                    case 3:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/AdminMenuD3.png"));
                        break;
                    case 4:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/AdminMenuD4.png"));
                        break;
                    case 5:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/AdminMenuD5.png"));
                        break;
                    case 6:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/AdminMenuD6.png"));

                        break;
                    case 7:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/AdminMenuD7.png"));

                        break;
                    case 8:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/AdminMenuD8.png"));

                        break;
                    case 9:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/AdminMenuD9.png"));

                        break;
                    case 10:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/AdminMenuD10.png"));

                        break;
                    case 11:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/AdminMenuD11.png"));

                        break;
                    case 12:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/AdminMenuD12.png"));

                        break;
                    case 13:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/AdminMenuD13.png"));

                        break;
                    case 14:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/AdminMenuD14.png"));

                        break;
                    case 15:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/AdminMenuD15.png"));

                        break;
                    case 16:
                        Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16.png"));
                        Global.MainWindow.LogoAdminMenu.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/AdminMenuD16.png"));

                        break;
                    default:
                        break;
                }

                Global.MainWindow.BtnHomeImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Home.png"));
                Global.MainWindow.BtnSearchImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Search.png"));
                Global.MainWindow.BtnShuffleImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Shuffle.png"));
                Global.MainWindow.BtnRepeatImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Repeat.png"));
                Global.MainWindow.BtnNextImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Next.png"));
                Global.MainWindow.BtnPreviousImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Previous.png"));
                Global.MainWindow.BtnPlaylistImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Playlist.png"));
                Global.MainWindow.BtnPlayImage.Source = new BitmapImage(new Uri($"pack://application:,,,/JukeBoxSolutions;component/Images/Theme{Global.AdminSettings.currentThemeSequence}/Play.png"));
                var btnLogo = Global.MainWindow.btnLogo;
                Global.MainWindow.MenuCarosel = new List<Page>() { new Menu1(ref mainFrame, ref btnLogo), new Settings(), new Pages.ImportModeSelection(), new Settings_Page1(), new Menu2(ref mainFrame), new ImportAnalytics(), new Pages.ImportModeSelection() };
                if (Global.AdminSettings.currentThemeSequence < 16)
                {
                    Global.AdminSettings.currentThemeSequence++;
                }
                else
                {
                    Global.AdminSettings.currentThemeSequence = 1;
                }

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var appSetting = db.AppSettings.FirstOrDefault(x => x.Type == "CurrentThemeSequnce");
                    if (appSetting != null)
                    {
                        appSetting.Value = Global.AdminSettings.currentThemeSequence.ToString();
                        db.Entry(appSetting).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.AppSettings.Add(new AppSetting
                        {
                            Type = "CurrentThemeSequnce",
                            Subtype = "",
                            Value = Global.AdminSettings.currentThemeSequence.ToString()
                        });
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/main_background.png"));

            }

            //try
            //{

            //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //{
            //    ResourceDictionary r = new ResourceDictionary();

            //    string colour = "";
            //    AppSetting ass;
            //    if (db.AppSettings.Where(w => w.Type == "ColourTheme").Any())
            //    {
            //        ass = db.AppSettings.Where(w => w.Type == "ColourTheme").First();
            //    }
            //    else
            //    {
            //        ass = new AppSetting() { Type = "ColourTheme", Subtype = "", Value = "NormalTheme" };
            //        db.AppSettings.Add(ass);
            //        db.SaveChanges();
            //    }

            //    colour = ass.Value;
            //        //switch (Global.AdminSettings.currentThemeSequence)
            //        //{
            //        //    case 1:
            //        //        r.Source = new Uri(Environment.CurrentDirectory + @"\Resources\Themes\BlackTheme.xaml");
            //        //        ass.Value = "BlackTheme";
            //        //        break;
            //        //    case "BlackTheme":
            //        //        r.Source = new Uri(Environment.CurrentDirectory + @"\Resources\Themes\PurpleTheme.xaml");
            //        //        ass.Value = "PurpleTheme";
            //        //        break;
            //        //    case "PurpleTheme":
            //        //        r.Source = new Uri(Environment.CurrentDirectory + @"\Resources\Themes\GreenTheme.xaml");
            //        //        ass.Value = "GreenTheme";
            //        //        break;
            //        //    case "GreenTheme":
            //        //        r.Source = new Uri(Environment.CurrentDirectory + @"\Resources\Themes\NormalTheme.xaml");
            //        //        ass.Value = "NormalTheme";
            //        //        break;
            //        //}

            //        switch (Global.AdminSettings.currentThemeSequence)
            //        {
            //            case 1:
            //                Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("/JukeBoxSolutions;component/Images/Theme1/main_background.png"));
            //                
            //                break;
            //            case 2:
            //                Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("/JukeBoxSolutions;component/Images/Theme2/Design2.png"));
            //                
            //                break;
            //            case 3:
            //                Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("/JukeBoxSolutions;component/Images/Theme3/Design3.png"));
            //                
            //                break;
            //            case 4:
            //                Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("/JukeBoxSolutions;component/Images/Theme4/Design4.png"));
            //                
            //                break;
            //            case 5:
            //                Global.MainWindow.MainBackgroundBG.Source = new BitmapImage(new Uri("/JukeBoxSolutions;component/Images/Theme5/Design5.png"));
            //                
            //                break;
            //            case 6:
            //                break;
            //            case 7:
            //                break;
            //            case 8:
            //                break;
            //            case 9:
            //                break;
            //            case 10:
            //                break;
            //            case 11:
            //                break;
            //            case 12:
            //                break;
            //            case 13:
            //                break;
            //            default:
            //                break;
            //        }

            //        db.Entry(ass).State = System.Data.Entity.EntityState.Modified;
            //    db.SaveChanges();

            //    try
            //    {
            //        int i = 0;
            //        foreach (var x in r.Values)
            //        {
            //            string k = r.Keys.OfType<string>().ElementAt(i);
            //            Application.Current.Resources[k] = x;
            //            i++;
            //        }
            //    }
            //    catch
            //    {
            //        Global.mPlayer.ShowNotification(String.Format("ERROR: Cannot find {0} Theme", ass.Value));
            //        Thread.Sleep(1000);
            //        Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            //    }
            //}

            //    //Global.MainWindow.InitializeComponent();
            //    //Global.MainWindow.ReloadControls();


            //    //if (isActive)
            //    //{
            //    //    r.Source = new Uri("/JukeBoxSolutions;component/Resources/Themes/ExternalTestDirectory.xaml", UriKind.RelativeOrAbsolute);
            //    //    isActive = false;
            //    //}
            //    //else
            //    //{
            //    //    r.Source = new Uri("/JukeBoxSolutions;component/Resources/Themes/GreenTheme.xaml", UriKind.RelativeOrAbsolute);

            //    //    //var foo = r.Values.OfType<LinearGradientBrush>().FirstOrDefault().GetType();
            //    //    //Application.Current.Resources["ControlGradientMain"] = foo;
            //    //    isActive = true; ;
            //    //}

            //    //try
            //    //{
            //    //    //r.Source = new Uri(Environment.CurrentDirectory + @"\Resources\Themes\BlackTheme.xaml");
            //    //    r.Source = new Uri(Environment.CurrentDirectory + @"\Resources\Themes\PurpleTheme.xaml");
            //    //}
            //    //catch
            //    //{

            //    //}

            //}
            //catch (Exception)
            //{

            //}

        }

        private void LoadWifi()
        {
            try
            {
                var a = NativeWifi.EnumerateConnectedNetworkSsids().Select(y => y.ToString());
                if (a.Any())
                {
                    btnWifi.Content = a.First();

                    var b = btnWifi.Background;
                    btnWifi.Background = Brushes.White;
                    btnWifi.Foreground = Brushes.Black;

                }
            }
            catch
            {
                btnWifi.Content = "Error in Wifi";
            }
        }

        private void btnWifi_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new WifiControl();
        }

        private void btnOnlineAccounts_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new OnlineAccountsControl();
        }

        private void btnSmartUpdater_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new Popup_SmartUpdater();
        }

        int ErrorCycle = 0;
        List<string> typedump = new List<string>();
        List<string> typeDistinct = new List<string>();
        int typeIndex = 0;
        private void btnTestImportErrors_Click(object sender, RoutedEventArgs e)
        {
            if (ErrorCycle == 0)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    typedump = (from b in db.TrackLibraries
                                select b.Type).ToList();
                    typeDistinct = typedump.Distinct().ToList();
                }

                if (typedump.Any())
                {
                    ErrorCycle++;
                    btnTestImportErrors.Content = string.Format("Found {0} types", typeDistinct.Count());
                }
                else
                    btnTestImportErrors.Content = "No Error Tracks";
            }
            else
            {
                int x = typedump.Count(c => c == typeDistinct[typeIndex]);
                string content = string.Format("{0} ({1})", typeDistinct[typeIndex], x);
                btnTestImportErrors.Content = content;

                if (ErrorCycle == typeDistinct.Count())
                {
                    ErrorCycle = 0;
                    typeIndex = 0;
                }
                else
                {
                    ErrorCycle++;
                    typeIndex++;
                }
            }
        }


        int AttentionCycle = 0;
        private void btnTestImportAttention_Click(object sender, RoutedEventArgs e)
        {
            if (Global.importFactory.MismatchSongBuffer.Any())
            {
                btnTestImportAttention.Content = Global.importFactory.MismatchSongBuffer.Count();
            }
            else
            {
                btnTestImportAttention.Content = "No Actions Needed";
            }
        }

        private void btnTestAttention_Click(object sender, RoutedEventArgs e)
        {
            Global.AlertNotifications = false;
            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new Popup_Choices();
        }

        public void btnResetDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    int r = 0;
                    r = db.Database.ExecuteSqlCommand("truncate table Map_Album_Songs");
                    r = db.Database.ExecuteSqlCommand("truncate table Map_Artist_Songs");
                    r = db.Database.ExecuteSqlCommand("truncate table Map_Files_Songs");

                    r = db.Database.ExecuteSqlCommand("truncate table iTunes_AlbumDetails");

                    r = db.Database.ExecuteSqlCommand("delete from LibraryView");

                    r = db.Database.ExecuteSqlCommand("delete from SongLibrary");
                    r = db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([SongLibrary], RESEED, 1);");

                    r = db.Database.ExecuteSqlCommand("delete from Artists");
                    r = db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Artists]);");

                    r = db.Database.ExecuteSqlCommand("delete from AlbumLibrary");
                    r = db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([AlbumLibrary]);");

                    r = db.Database.ExecuteSqlCommand("truncate table AppSettings");

                    r = db.Database.ExecuteSqlCommand("delete from Playlists where [PlaylistId] <> 1");

                    r = db.Database.ExecuteSqlCommand("delete from PlayListDetails where [Type] = 1");

                    r = db.Database.ExecuteSqlCommand("delete from TrackLibrary where [TrackLibrary].Id not in (select TrackId from [Playlists])");

                    r = db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Playlists]);");
                    r = db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([PlayListDetails]);");
                    r = db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([TrackLibrary]);");

                    r = db.Database.ExecuteSqlCommand("select count(*) from TrackLibrary");

                    db.SaveChanges();

                    r = db.SongLibraries.Count();
                    r = db.Playlists.Count();
                    r = db.TrackLibraries.Count();
                    Global.MainWindow.CurrentLibraryInstnace = null;
                    Global.MainWindow.CurrentKaraokeLibraryInstnace = null;
                    Global.MainWindow.CurrentRadioLibraryInstnace = null;
                    Global.MainWindow.CurrentVideoLibraryInstnace = null;
                    Global.MainWindow.KaraokeLibraryInstancesLoaded = false;
                    Global.MainWindow.MusicLibraryInstancesLoaded = false;
                    Global.MainWindow.RadioLibraryInstancesLoaded = false;
                    Global.MainWindow.VideoLibraryInstancesLoaded = false;
                    Global.mPlayer.ShowNotification("Databse is now reset to Factory Settings");
                    Thread.Sleep(1000);
                    Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                }
            }
            catch (Exception)
            {

            }

        }

        private void btnResumeUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnResumeUpdate.IsEnabled = false;

            //string lines = System.IO.File.ReadAllText(@"C:\Creative Projects\WhatsappNumbers.txt");
            //List<string> extractedNumbers = new List<string>();
            //StringBuilder sb = new StringBuilder();
            //// opening
            //int i = 0;

            //bool keepsearching = true;
            //while (keepsearching)
            //{
            //    int i2 = lines.IndexOf(">+27", i);
            //    if (i2 == -1)
            //    {
            //        keepsearching = false;
            //        break;
            //    }

            //    i2++;
            //    string t = lines.Substring(i2, 15);

            //    i = i2 + 15;
            //    int Nexti2 = lines.IndexOf(">+27", i) + 1;
            //    string searchtext = @"class=""_1qB8f""><span";
            //    int i3 = lines.IndexOf(searchtext, i);

            //    string t2 = lines.Substring(i2, 12);

            //    if (i3 > Nexti2)
            //    {
            //        // skip to next entry
            //        extractedNumbers.Add(t);
            //    }
            //    else
            //    {
            //        int i4 = lines.IndexOf(">", i3 + searchtext.Length) + 1;
            //        int i5 = lines.IndexOf("<", i4) - i4;
            //        t2 = lines.Substring(i4, i5);

            //        extractedNumbers.Add(t + " - " + t2);
            //    }
            //}

            //extractedNumbers = extractedNumbers.Distinct().ToList();

            //foreach (string s in extractedNumbers)
            //    sb.AppendLine(s);

            //string file = @"C:\Creative Projects\WhatsappNumbers_extracted.txt";
            //System.IO.File.WriteAllText(file, sb.ToString());


            List<ImportFactory.ManagedFile> list = new List<ImportFactory.ManagedFile>();
            List<TrackLibrary> templist = new List<TrackLibrary>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                if (db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId).Any())
                {
                    // kick off import all
                    // create managed files

                    bool isBusyLoading = true;
                    // if in db, then add to playlist
                    templist = (from l in db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId)
                                select l.TrackLibrary).ToList();
                }

                foreach (var l in templist)
                {
                    list.Add(new ImportFactory.ManagedFile(l));
                }
            }

            if (list.Any())
            {
                // clear db
                Global.mPlayer.dbRemoveTrackFromPlaylist(templist.ToList(), Global.AutoUpdateTodoListPlaylistId);
                iTunesSearchManager iManager = new iTunesSearchManager();
                iManager.AutoUpdateTreadedTracksV2(list.ToList());
            }
        }

        private void btnResumeImport_Click(object sender, RoutedEventArgs e)
        {

            if (Global.importFactory.ResumeImportKaraokeFiles())
            {
                btnResumeImport.Content = "Imported Karaoke";
            }
            else if (Global.importFactory.ResumeImportMusicFiles())
            {
                btnResumeImport.Content = "Imported Music";
            }
            else if (Global.importFactory.ResumeImportVideoFiles())
            {
                btnResumeImport.Content = "Imported Video";
            }
            else
            {
                btnResumeImport.Content = "Nothing";
                btnResumeImport.IsEnabled = false;
            }
            //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //{
            //    var tracks = db.TrackLibraries.Where(w => w.Type == "Import - Video");

            //    if (tracks.Any())
            //    {

            //    }
            //}
        }

        private void btnExportMetaData_Click(object sender, RoutedEventArgs e)
        {
            // Export Music Videos
            // Create Info Files

            // Get tracks... export data
            ExportFactory f = new ExportFactory();
            List<TrackLibrary> trackbatch = new List<TrackLibrary>();
            //f.ExportFolder("")

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var x = db.Playlists.Where(w => w.PlaylistId == Global.SystemPlaylistID).Select(s => s.TrackId);

                List<string> census_data = new List<string>();

                trackbatch = (from track in db.TrackLibraries
                              where track.Type == "Video" && !x.Contains(track.Id)
                              select track).ToList();


                //var folders = (from folder in r
                //               select new { 
                //               folder = folder.FilePath.Replace(folder.FileName + folder.Extention, ""),
                //               filepath = folder.FilePath,
                //               artists = folder.SongLibraries.SelectMany(art=> art.Artists),
                //               albums = folder.SongLibraries.SelectMany(alb=>alb.AlbumLibraries)
                //               }).OrderBy(o=> o.folder).ToList().Distinct();

                //var foldersD = folders.Select(s => s.folder).Distinct();


                //var buffer = new StringBuilder();


                //foreach (var i in foldersD)
                //{
                //    var batch = r.Where(b => b.FilePath == i + b.FileName + b.Extention);

                //    var minifolder = folders.Where(w => w.folder == i);
                //    var artistbuffer = minifolder.SelectMany(art => art.artists).Distinct();
                //    var albumbuffer = minifolder.SelectMany(alb => alb.albums).Distinct();

                //    int index = 0;
                //    foreach (var a in albumbuffer)
                //    {
                //        buffer.AppendLine(String.Format("AlbumDetails[{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}]", index, a.AlbumName, a.MusicBrainzId, a.Year, a.NumTracks, a.isVerified, a.hasMusicBrainzUpdate, a.iTunesId, a.hasiTunesUpdate, a.isHidden));
                //        index++;
                //    }

                //    index = 0;
                //    foreach (var a in artistbuffer)
                //    {
                //        buffer.AppendLine(String.Format("ArtistDetails[{0}|{1}|{2}|{3}|{4}]", index, a.ArtistName, a.iTunesId, a.isVerified, a.hasiTunesUpdate));
                //        index++;
                //    }

                //    batch.ToList().ForEach(item => buffer.AppendLine(String.Format("TrackDetails[{0}|{1}|{2}|{3}|{4}]", item.FilePath, item.SongLibraries.First().SongName, item.SongLibraries.First().Genre, 0, 0)));
                //}




                //        var result =
                //from c in census_data
                //group c by c.state into g
                //join s in state_gdp on g.FirstOrDefault().state equals s.state
                //orderby s.gdp descending
                //select new
                //{
                //    State = g.Key,
                //    Count = g.Count(),
                //    SavingsBalance = g.Average(x => x.savingsBalanceDouble),
                //    GDP = s.gdp
                //};

                //buffer.AppendLine("#key,name,sum,gdp");
                //if (r.Any())
                //    r.ToList().ForEach(item => buffer.AppendLine(String.Format("TrackDetails[{0}|{1}|{2}|{3}|{4}]", item.FilePath, item.SongLibraries.First().SongName, item.SongLibraries.First().Genre, 0, 0)));


                //result.ToList().ForEach(item => buffer.AppendLine(String.Format("{0},{1},{2},{3}", item.State, item.Count, item.SavingBalance, item.GDP)));


                //File.WriteAllText("c:\\temp\\file.csv", buffer.ToString());

            }


            f.ExportTracks(trackbatch);
            // Save Album /JukeBoxSolutions;component/Images

            btnExportMetaData.IsEnabled = false;
        }

        private void btnForceAlbumCovers_Click(object sender, RoutedEventArgs e)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                //var albums = db.AlbumLibraries.Where(a => (a.CoverArt == null) && a.isHidden == false).ToList();
                foreach (var al in db.AlbumLibraries.Where(w => w.AlbumName == "Old Skool"))
                {
                    string name = al.AlbumName;
                    byte[] albumart = al.CoverArt;

                    bool isNull = false;
                    bool isLeng = false;

                    if (al.CoverArt == null)
                        isNull = true;
                    else if (al.CoverArt.Length == 0)
                        isLeng = true;
                }


                var albums = (from a in db.AlbumLibraries.Where(w => w.isHidden == false) where a.CoverArt == null select a).ToList();
                // get folders from tracks... where all tracks fill in a folder?
                foreach (var a in albums)
                {
                    var tracks = a.SongLibraries.SelectMany(s2 => s2.TrackLibraries).Distinct();

                    var folderpaths = (from f in tracks select f.FilePath.Replace(f.FileName + f.Extention, "")).Distinct();

                    if (folderpaths.Count() == 1)
                    {
                        string path = folderpaths.First();
                        string searchPatternExpression = @"\.jpg|\.jpeg|\.png";
                        SearchOption searchOption = SearchOption.TopDirectoryOnly;
                        Regex reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);

                        var files = Directory.EnumerateFiles(path, "*", searchOption).Where(file => reSearchPattern.IsMatch(System.IO.Path.GetExtension(file)));
                    }
                }
            }
        }

        private void btnForceViewUpdate_Click(object sender, RoutedEventArgs e)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                //db.Database.ExecuteSqlCommand("exec BuildLibraryView");
                Global.ViewLoaded = true;
            }

            btnForceViewUpdate.IsEnabled = false;
        }





        //Global.AppMode = "Karaoke";
        //    //btnMenuKaraoke.Content = 
        //    mainFrame.Content = new Library(ref mainFrame);


    }
}
