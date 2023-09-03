using JukeBoxSolutions.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for favouriteItemsPopup.xaml
    /// </summary>
    public partial class favouriteItemsPopup : UserControl
    {
        private List<Library.PlayListSource> _albums;
        private List<Library.PlayListSource> _favAlbums=new List<Library.PlayListSource>();
        public favouriteItemsPopup(List<Library.PlayListSource> albums)
        {
            InitializeComponent();
            _albums = albums;
            PopulateAlbumList(albums);
        }

        private void PopulateAlbumList(List<Library.PlayListSource> albums)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    ItemsContainer.Children.Clear();
                    int count = 0;
                    foreach (var al in albums)
                    {
                        var result = db.Database.SqlQuery<bool>($"EXEC [dbo].[sp_checkIfFaourite] @AlbumID = {al.SourceAlbumLibrary.AlbumId}").ToList();
                        if (result[0] == false)
                        {
                            continue;
                        }
                        _favAlbums.Add(al);
                        count++;
                        string artistname = "";
                        var tempArt = db.SongLibraries.Find(al.SourceTracks.First().SongId);
                        if (tempArt.Artists.Any())
                        {
                            artistname = tempArt.Artists.First().ArtistName;
                        }

                        //Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, Parent, isSelected);
                        Controls.FavItemBar card = new Controls.FavItemBar(al.SourceAlbumLibrary, artistname,count);
                        card.Margin = new Thickness(0, 0, 10, 0);
                        ItemsContainer.Children.Add(card);
                    }


                    BtnAlbumCount.Content = count;
                    if (ItemsContainer.Children.Count == 0)
                    {
                        BtnPlayAll.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BtnPlayAll.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {

            }
            
        }

        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.popUp_Frame.Content= null;
            Global.mPlayer.popUp_Grid.Visibility=Visibility.Collapsed;
        }

        private void PlayAllAlbums(object sender, RoutedEventArgs e)
        {
            try
            {              
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (_favAlbums.Count == 0) { return; }
                    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // Get tracks
                        var _favAlbum = _favAlbums.First();
                        albumtracks = (from x in db.AlbumLibraries.Find(_favAlbum.SourceAlbumLibrary.AlbumId).SongLibraries
                                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                        Global.MainWindow.btnLogo.IsEnabled = false;
                        Global.MainWindow.btnKeyboard.IsEnabled = false;
                        Global.MainWindow.btnMediaPanel.IsEnabled = false;
                        var songPlayingArtworkcontrol = new SongPlayingArtwork(db.AlbumLibraries.Find(_favAlbum.SourceAlbumLibrary.AlbumId), albumtracks,null);
                        Global.MainFrame.Content = songPlayingArtworkcontrol;
                        Global.MainWindow.sParentPanel.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnKeyboard.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnMediaPanel.Visibility = Visibility.Collapsed;
                        Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
                        if (Global.AdminSettings.AudioVisualizerSize != Global.ClassAdminSettings.Quality.Off)
                        {
                            Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
                        }
                        Global.MainWindow.ControlPanelBG.Visibility = Visibility.Collapsed;
                        Global.MainWindow.PlaylistControlPanel.Height = 120;
                        //Global.MainWindow.VolumeTracker.Value = 70;
                        Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                        Global.mPlayer.popUp_Frame.Content = null;
                        Global.mPlayer.SongPlayingArtWork = songPlayingArtworkcontrol;
                        if (Global.AdminSettings.VolumeIncrement == 40)
                        {
                            Global.MainWindow.VolControlPanel.volLevel1_Click(null, null);
                        }
                        else if (Global.AdminSettings.VolumeIncrement == 80)
                        {
                            Global.MainWindow.VolControlPanel.volLevel2_Click(null, null);
                        }
                        else if (Global.AdminSettings.VolumeIncrement == 120)
                        {
                            Global.MainWindow.VolControlPanel.volLevel3_Click(null, null);
                        }
                        else if (Global.AdminSettings.VolumeIncrement == 160)
                        {
                            Global.MainWindow.VolControlPanel.volLevel4_Click(null, null);
                        }
                        else if (Global.AdminSettings.VolumeIncrement == 200)
                        {
                            Global.MainWindow.VolControlPanel.volLevel5_Click(null, null);
                        }
                        Global.mPlayer.playList.PlayIndex = 0;
                        Global.mPlayer.PlayNow(albumtracks.ToList());
                        Global.AppActionMode = Global.AppActionModeEnum.Playing;
                        Global.MainWindow.btnPlay1.IsEnabled = true;
                        Global.MainWindow.btnPlay1.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnPause.IsEnabled = true;
                        Global.MainWindow.btnPause.Visibility = Visibility.Visible;
                        //Global.MainWindow.VolumnPanel.Visibility = Visibility.Visible;
                        Global.MainWindow.btnLogo.IsEnabled = true;
                        Global.MainWindow.btnKeyboard.IsEnabled = true;
                        Global.MainWindow.btnMediaPanel.IsEnabled = true;
                        Global.MainWindow.btnPreviousMenu.Visibility = Visibility.Visible;
                        Global.MainWindow.btnNextMenu.Visibility = Visibility.Visible;
                        Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
                    }
                });
                
            }
            catch (Exception)
            {

            }
        }
    }
}
