using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media.Animation;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistPopup.xaml
    /// </summary>
    public partial class PlaylistPopup : UserControl
    {
        public int currentPageIndex { get; set; } = 0;
        public int totalMusicCount { get; set; } = 0;

        public int pageCount = 10;

        public List<Playlist> PlaylistToRemove { get; set; } = new List<Playlist>();
        public List<Playlist> RearrangedPlaylists { get; set; } = new List<Playlist>();
        public List<PlaylistCardSong> Cards { get; set; } = new List<PlaylistCardSong>();

        public List<int> idToRemove { get; set; } = new List<int>();
        public bool IsClearAll { get; set; }
        public List<PlaylistWithActionTaken> PLaylists { get; set; } = new List<PlaylistWithActionTaken>();
        public PlaylistPopup()
        {
            InitializeComponent();
            PopulateSonglist();
        }

        public void PopulateSonglist()
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var playlist = db.Playlists.ToList();
                    RearrangedPlaylists.AddRange(playlist);
                    int i = 0;
                    foreach (var item in RearrangedPlaylists)
                    {
                        var trackLib = item.TrackLibrary;
                        var card = new PlaylistCardSong(trackLib, (i + 1).ToString(), parent: this);
                        Cards.Add(card);
                        i++;
                    }
                    var totalMusicCount = playlist.Count;
                    var playlists = Cards.ToList().Skip(10 * currentPageIndex).Take(10).ToList();
                    
                    foreach (var item in playlists)
                    {
                        stackSongItems.Children.Add(item);
                       
                    }

                    BtnAlbumCount.Content = totalMusicCount;
                    this.totalMusicCount = totalMusicCount;
                }
            }
            catch (Exception)
            {

            }
        }

        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnPrevious_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (currentPageIndex != 0)
                {
                    currentPageIndex--;
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var playlists = Cards.Skip(10 * currentPageIndex).Take(10).ToList();
                        if (playlists.Count > 0)
                        {
                            stackSongItems.Children.Clear();
                            int i = (10 * currentPageIndex);
                            foreach (var item in playlists)
                            {
                                stackSongItems.Children.Add(item);
                                i++;
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        private void btnNext_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (10 * (currentPageIndex + 1) < totalMusicCount)
                {
                    currentPageIndex++;
                    var playlists = Cards.Skip(10 * currentPageIndex).Take(10).ToList();
                    if (playlists.Count > 0)
                    {
                        stackSongItems.Children.Clear();
                        int i = (10 * currentPageIndex);
                        foreach (var item in playlists)
                        {
                            stackSongItems.Children.Add(item);
                            i++;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void BtnCancelPopup(object sender, System.Windows.RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
            Global.mPlayer.popUp_Frame.Content = null;
        }

        private void BtnDonePopup(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (!IsClearAll)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        db.Database.ExecuteSqlCommand("delete from Playlists where [PlaylistId] <> 1");
                        db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Playlists]);");
                        db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([PlayListDetails]);");
                        db.SaveChangesAsync();
                        foreach (var item in Cards)
                        {
                            if (idToRemove.Count(x => x == item.baseTrack.Id) > 0) continue;
                            Global.mPlayer.dbAddTrackToPlaylist(item.baseTrack, Global.NowPlayingId);
                        }

                    }

                    Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                    Global.mPlayer.popUp_Frame.Content = null;

                    if (Global.MainWindow.MusicLibraryFrame != null)
                    {
                        var content = Global.MainWindow.MusicLibraryFrame.Content as Library;
                        if (content != null)
                        {
                            foreach (var child in content.UniGridHorz.Children)
                            {
                                var baseControlCard = child as LibraryCard;
                                if (baseControlCard.CheckIfAddedToPlaylist())
                                {
                                    baseControlCard.AddIcon.Visibility = Visibility.Collapsed;
                                    baseControlCard.AddIconBG.Visibility = Visibility.Collapsed;
                                    baseControlCard.RemoveIcon.Visibility = Visibility.Visible;
                                    baseControlCard.RemoveIconBG.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    baseControlCard.AddIcon.Visibility = Visibility.Visible;
                                    baseControlCard.AddIconBG.Visibility = Visibility.Visible;
                                    baseControlCard.RemoveIcon.Visibility = Visibility.Collapsed;
                                    baseControlCard.RemoveIconBG.Visibility = Visibility.Collapsed;
                                }
                            }
                            foreach (var child in content.UniGridVert.Children)
                            {
                                var baseControlCard = child as LibraryCard;
                                if (baseControlCard.CheckIfAddedToPlaylist())
                                {
                                    baseControlCard.AddIcon.Visibility = Visibility.Collapsed;
                                    baseControlCard.AddIconBG.Visibility = Visibility.Collapsed;
                                    baseControlCard.RemoveIcon.Visibility = Visibility.Visible;
                                    baseControlCard.RemoveIconBG.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    baseControlCard.AddIcon.Visibility = Visibility.Visible;
                                    baseControlCard.AddIconBG.Visibility = Visibility.Visible;
                                    baseControlCard.RemoveIcon.Visibility = Visibility.Collapsed;
                                    baseControlCard.RemoveIconBG.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }

                    if (Global.MainWindow.KaraokeLibraryFrame != null)
                    {
                        var content = Global.MainWindow.KaraokeLibraryFrame.Content as Library;
                        if (content != null)
                        {
                            var childs = content.SongsGridLibraryVertical.Children;
                            if (childs != null && childs.Count > 0)
                            {
                                var panel = childs[1] as StackPanel;
                                if (panel != null && panel.Children?.Count > 0)
                                {
                                    foreach (var itema in panel.Children.OfType<UniformGrid>())
                                    {
                                        foreach (var item1 in itema.Children)
                                        {
                                            var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                            if (cardSong != null)
                                            {
                                                cardSong.showAddToPlaylistx = cardSong.CheckIfAddedInPlaylist(cardSong.baseTrack);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Global.MainWindow.VideoLibraryFrame != null)
                    {
                        var content = Global.MainWindow.VideoLibraryFrame.Content as Library;
                        if (content != null)
                        {
                            var childs = content.SongsGridLibraryVertical.Children;
                            if (childs != null && childs.Count > 0)
                            {
                                var panel = childs[1] as StackPanel;
                                if (panel != null && panel.Children?.Count > 0)
                                {
                                    foreach (var itema in panel.Children.OfType<UniformGrid>())
                                    {
                                        foreach (var item1 in itema.Children)
                                        {
                                            var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                            if (cardSong != null)
                                            {
                                                cardSong.showAddToPlaylistx = cardSong.CheckIfAddedInPlaylist(cardSong.baseTrack);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        db.Database.ExecuteSqlCommand("delete from Playlists where [PlaylistId] <> 1");
                        db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Playlists]);");
                        db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([PlayListDetails]);");
                        db.SaveChangesAsync();
                        Global.mPlayer.Stop();
                        Global.mPlayer.SongPlayingArtWork = null;
                        Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Collapsed;
                        Global.mPlayer.popUp_Frame.Content = null;
                        
                        if (Global.MainWindow.MusicLibraryFrame != null)
                        {
                            var content = Global.MainWindow.MusicLibraryFrame.Content as Library;
                            if (content != null)
                            {
                                foreach (var child in content.UniGridHorz.Children)
                                {
                                    var songCard = child as LibraryCard;
                                    if (songCard != null)
                                    {
                                        songCard.AddBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.RemoveBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.AddIcon.Visibility = System.Windows.Visibility.Visible;
                                        songCard.AddIconBG.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.RemoveIcon.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.RemoveIconBG.Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                }
                                foreach (var child in content.UniGridVert.Children)
                                {
                                    var songCard = child as LibraryCard;
                                    if (songCard != null)
                                    {
                                        songCard.AddBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.RemoveBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.AddIcon.Visibility = System.Windows.Visibility.Visible;
                                        songCard.AddIconBG.Visibility = System.Windows.Visibility.Visible;
                                        songCard.RemoveIcon.Visibility = System.Windows.Visibility.Collapsed;
                                        songCard.RemoveIconBG.Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                }
                            }
                        }
                        if (Global.MainWindow.KaraokeLibraryFrame != null)
                        {
                            var content = Global.MainWindow.KaraokeLibraryFrame.Content as Library;
                            if (content != null)
                            {
                                var childs = content.SongsGridLibraryVertical.Children;
                                if (childs != null && childs.Count > 0)
                                {
                                    var panel = childs[1] as StackPanel;
                                    if (panel != null && panel.Children?.Count > 0)
                                    {
                                        foreach (var item in panel.Children.OfType<UniformGrid>())
                                        {
                                            foreach (var item1 in item.Children)
                                            {
                                                var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                                if (cardSong != null)
                                                {
                                                    cardSong.showAddToPlaylistx = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Global.MainWindow.VideoLibraryFrame != null)
                        {
                            var content = Global.MainWindow.VideoLibraryFrame.Content as Library;
                            if (content != null)
                            {
                                var childs = content.SongsGridLibraryVertical.Children;
                                if (childs != null && childs.Count > 0)
                                {
                                    var panel = childs[1] as StackPanel;
                                    if (panel != null && panel.Children?.Count > 0)
                                    {
                                        foreach (var item in panel.Children.OfType<UniformGrid>())
                                        {
                                            foreach (var item1 in item.Children)
                                            {
                                                var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                                if (cardSong != null)
                                                {
                                                    cardSong.showAddToPlaylistx = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
            catch (Exception)
            {

            }
        }

        private void ClearPlaylist(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            { 
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Frame.Content = null;
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    db.Database.ExecuteSqlCommand("delete from Playlists where [PlaylistId] <> 1");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Playlists]);");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([PlayListDetails]);");
                    db.SaveChangesAsync();
                    Global.mPlayer.Stop();
                    Global.mPlayer.SongPlayingArtWork = null;
                    Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Collapsed;
                    Global.mPlayer.popUp_Frame.Content = null;
                    Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Visible;
                    Global.MainWindow.btnLogo_Click(null, null);
                    if (Global.MainWindow.MusicLibraryFrame != null)
                    {
                        var content = Global.MainWindow.MusicLibraryFrame.Content as Library;
                        if (content != null)
                        {
                            foreach (var child in content.UniGridHorz.Children)
                            {
                                var songCard = child as LibraryCard;
                                if (songCard != null)
                                {
                                    songCard.AddBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.RemoveBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.AddIcon.Visibility = System.Windows.Visibility.Visible;
                                    songCard.AddIconBG.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.RemoveIcon.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.RemoveIconBG.Visibility = System.Windows.Visibility.Collapsed;
                                }
                            }
                            foreach (var child in content.UniGridVert.Children)
                            {
                                var songCard = child as LibraryCard;
                                if (songCard != null)
                                {
                                    songCard.AddBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.RemoveBtnStack.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.AddIcon.Visibility = System.Windows.Visibility.Visible;
                                    songCard.AddIconBG.Visibility = System.Windows.Visibility.Visible;
                                    songCard.RemoveIcon.Visibility = System.Windows.Visibility.Collapsed;
                                    songCard.RemoveIconBG.Visibility = System.Windows.Visibility.Collapsed;
                                }
                            }
                        }
                    }
                    if (Global.MainWindow.KaraokeLibraryFrame != null)
                    {
                        var content = Global.MainWindow.KaraokeLibraryFrame.Content as Library;
                        if (content != null)
                        {
                            var childs = content.SongsGridLibraryVertical.Children;
                            if (childs != null && childs.Count > 0)
                            {
                                var panel = childs[1] as StackPanel;
                                if (panel != null && panel.Children?.Count > 0)
                                {
                                    foreach (var item in panel.Children.OfType<UniformGrid>())
                                    {
                                        foreach (var item1 in item.Children)
                                        {
                                            var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                            if (cardSong != null)
                                            {
                                                cardSong.showAddToPlaylistx = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Global.MainWindow.VideoLibraryFrame != null)
                    {
                        var content = Global.MainWindow.VideoLibraryFrame.Content as Library;
                        if (content != null)
                        {
                            var childs = content.SongsGridLibraryVertical.Children;
                            if (childs != null && childs.Count > 0)
                            {
                                var panel = childs[1] as StackPanel;
                                if (panel != null && panel.Children?.Count > 0)
                                {
                                    foreach (var item in panel.Children.OfType<UniformGrid>())
                                    {
                                        foreach (var item1 in item.Children)
                                        {
                                            var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                            if (cardSong != null)
                                            {
                                                cardSong.showAddToPlaylistx = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }


}
