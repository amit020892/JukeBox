using Hqub.MusicBrainz.API.Entities;
using iTunesSearch.Library.Models;
using JukeBoxSolutions.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static JukeBoxSolutions.Library;
using static JukeBoxSolutions.Library.PlayListSource2;
using static JukeBoxSolutions.MainPlayer;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for SongPlayingArtwork.xaml
    /// </summary>
    public partial class SongPlayingArtwork : Page
    {
        public int currentSelectedIndex = 0;
        public int currentPlayingIndex = 0;
        public int totalMusicCount = 0;
        int currentArtWorkIndex = 0;

        public List<TrackLibrary> AlbumTracks { get; set; }
        public AlbumLibrary AlbumLibrary { get; set; }
        public LibraryCard BaseControlCard { get; set; }
        public TrackLibrary CurrentTrack { get; set; }

        public bool _isCarouselVisible { get; set; }
        public CarouselUserControl CarouselUserControl { get; set; }
        public SongPlayingArtwork(AlbumLibrary allAlbum, List<TrackLibrary> albumtracks, LibraryCard baseControlCard, bool isCarouselVisible = true)
        {
            InitializeComponent();
            //Global.MainWindow.BtnNext10.IsEnabled = true;
            AlbumLibrary = allAlbum;
            AlbumTracks = albumtracks;
            totalMusicCount = albumtracks.Count;
            BaseControlCard = baseControlCard;
            _isCarouselVisible = false;
            if (_isCarouselVisible)
            {
                btnNextMenu.Visibility = Visibility.Visible;
                btnPreviousMenu.Visibility = Visibility.Visible;
                Task.WhenAll(this.LoadArtwork(allAlbum, albumtracks.Skip(10 * currentArtWorkIndex).Take(10).ToList()), PlayTracks(albumtracks));
            }
            else
            {
                btnNextMenu.Visibility = Visibility.Collapsed;
                btnPreviousMenu.Visibility = Visibility.Collapsed;
                PlayTracks(albumtracks);
            }

        }

        public SongPlayingArtwork(List<TrackLibrary> trackLibraries)
        {
            InitializeComponent();
            this.AlbumTracks = trackLibraries;
            PlayTracks(trackLibraries, isFromPlaylist: true);
            this.btnNextMenu.Visibility = Visibility.Collapsed;
            this.btnPreviousMenu.Visibility = Visibility.Collapsed;
        }

        public void UpdateTrackList(AlbumLibrary allAlbum, List<TrackLibrary> albumtracks)
        {
            AlbumLibrary = allAlbum;
            AlbumTracks = albumtracks;
            Task.WhenAll(this.LoadArtwork(allAlbum, albumtracks.Skip(10 * currentArtWorkIndex).Take(10).ToList()), PlayTracks(albumtracks));
        }

        public Task PlayTracks(List<TrackLibrary> albumtracks, bool isFromPlaylist = false)
        {
            try
            {
                Global.mPlayer.SongPlayingArtWork = this;
                Global.mPlayer.playList.PlayIndex = 0;
                var currentTrack = albumtracks[0];
                CurrentTrack = albumtracks[0];
                setCurrentTrackInfo(currentTrack, isFromPlaylist);
                Global.mPlayer.Play(currentTrack);
                Global.AppActionMode = Global.AppActionModeEnum.Playing;
            }
            catch (Exception ex)
            {

            }

            return Task.FromResult(true);
        }

        private void setCurrentTrackInfo(TrackLibrary currentTrack, bool isFromPlaylist = false)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    if (isFromPlaylist)
                    {
                        Global.mPlayer.CurrentTrackInfo = new CurrentTrackInfo
                        {
                            SongName = currentTrack.FileName,
                        };
                    }
                    else
                    {
                        var artists = (from x in db.AlbumLibraries.Find(AlbumLibrary.AlbumId).SongLibraries
                                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                       select x.Artists.FirstOrDefault()).ToList();

                        Global.mPlayer.CurrentTrackInfo = new CurrentTrackInfo
                        {
                            AlbumName = AlbumLibrary.AlbumName,
                            SongName = currentTrack.FileName,
                            ArtistName = artists.First().ArtistName,
                        };
                    }



                }
            }
            catch (Exception)
            {

            }
        }

        private Task LoadArtwork(AlbumLibrary allAlbum, List<TrackLibrary> albumtracks)
        {
            try
            {
                this.ArtWorkGrid.Children.Clear();
                CarouselUserControl = new CarouselUserControl();
                var list = new List<RadioStation>();
                foreach (var al in albumtracks)
                {
                    var radioSt = new RadioStation();
                    radioSt.ID = al.Id;
                    radioSt.Name = al.FileName;
                    radioSt.ShortName = allAlbum.AlbumName;
                    radioSt.ImageSource = LoadCover(allAlbum);
                    list.Add(radioSt);
                }

                CarouselUserControl._carouselDABRadioStations.ItemsSource = list;

                this.ArtWorkGrid.Children.Add(CarouselUserControl);
            }
            catch (Exception)
            {

            }

            return Task.FromResult(true);
        }

        //public void _buttonLeftArrow_Click(object sender, RoutedEventArgs e)
        //{
        //    _carouselDABRadioStations.RotateRight();
        //    var selectedTrack = _carouselDABRadioStations.SelectedItem as RadioStation;
        //    if (selectedTrack != null)
        //    {
        //        var itemSources = _carouselDABRadioStations.ItemsSource as List<RadioStation>;
        //        Global.mPlayer.playList.PlayIndex = itemSources.FindIndex(x => x.Name == selectedTrack.Name);
        //        Global.mPlayer.Play(Global.mPlayer.playList[Global.mPlayer.playList.PlayIndex]);
        //    }
        //}

        //public void _buttonRightArrow_Click(object sender, RoutedEventArgs e)
        //{
        //    _carouselDABRadioStations.RotateLeft();
        //    var selectedTrack = _carouselDABRadioStations.SelectedItem as RadioStation;
        //    if (selectedTrack != null)
        //    {
        //        var itemSources = _carouselDABRadioStations.ItemsSource as List<RadioStation>;
        //        Global.mPlayer.playList.PlayIndex=itemSources.FindIndex(x=>x.Name==selectedTrack.Name);
        //        Global.mPlayer.Play(Global.mPlayer.playList[Global.mPlayer.playList.PlayIndex]);
        //    }
        //}


        public void PlayPrevious()
        {
            try
            {
                if (_isCarouselVisible)
                {
                    if (currentSelectedIndex == 0)
                    {
                        currentSelectedIndex = totalMusicCount - 1;
                    }
                    else
                    {
                        currentSelectedIndex--;
                    }

                    CarouselUserControl._carouselDABRadioStations.RotateRight();
                    var trackId = (CarouselUserControl._carouselDABRadioStations.ItemsSource as List<RadioStation>)[currentSelectedIndex].ID;
                    var track = AlbumTracks.FirstOrDefault(x => x.Id == trackId);
                    if (track != null)
                    {
                        Global.mPlayer.Play(track);
                        setCurrentTrackInfo(track);
                    }
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var list = db.Playlists.ToList();
                        var playlistIndex = list.IndexOf(list.First(x => x.TrackId == CurrentTrack.Id));
                        if (playlistIndex - 1 >= 0)
                        {
                            CurrentTrack = list[playlistIndex - 1].TrackLibrary;
                            Global.mPlayer.Play(list[playlistIndex - 1].TrackLibrary);
                            setCurrentTrackInfo(list[playlistIndex - 1].TrackLibrary, true);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

        }
        public void PlayNext()
        {
            try
            {
                if (_isCarouselVisible)
                {
                    if (currentSelectedIndex == totalMusicCount - 1)
                    {
                        currentSelectedIndex = 0;
                    }
                    else
                    {
                        currentSelectedIndex++;
                        CarouselUserControl._carouselDABRadioStations.RotateLeft();
                        var trackId = (CarouselUserControl._carouselDABRadioStations.ItemsSource as List<RadioStation>)[currentSelectedIndex].ID;
                        var track = AlbumTracks.FirstOrDefault(x => x.Id == trackId);
                        if (track != null)
                        {
                            Global.mPlayer.Play(track);
                            setCurrentTrackInfo(track);
                        }
                    }
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var list = db.Playlists.ToList();
                        var playlistIndex = list.IndexOf(list.First(x => x.TrackId == CurrentTrack.Id));
                        if (list.Count >= playlistIndex + 1)
                        {
                            CurrentTrack = list[playlistIndex + 1].TrackLibrary;
                            Global.mPlayer.Play(list[playlistIndex + 1].TrackLibrary);
                            setCurrentTrackInfo(list[playlistIndex + 1].TrackLibrary, true);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }

        }
        public BitmapImage LoadCover(AlbumLibrary baseAlbum)
        {
            System.Drawing.Bitmap bmp;
            BitmapImage AlbumCover = new BitmapImage();
            using (var ms = new MemoryStream(baseAlbum.CoverArt))
            {
                AlbumCover.BeginInit();
                AlbumCover.CacheOption = BitmapCacheOption.OnLoad;
                AlbumCover.StreamSource = ms;
                AlbumCover.EndInit();

                // AlbumImage.Source = AlbumCover;
                //buttonImage.Source = AlbumCover;
                return AlbumCover;
            }
        }

        private void _buttonLeftArrow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentArtWorkIndex > 0)
                {
                    currentArtWorkIndex--;

                    var nextAlbumTracks = AlbumTracks.Skip(10 * currentArtWorkIndex).Take(10).ToList();
                    if (nextAlbumTracks.Any())
                    {
                        this.LoadArtwork(AlbumLibrary, nextAlbumTracks);
                        Global.mPlayer.SongPlayingArtWork = this;
                        Global.mPlayer.playList.PlayIndex = 0;
                        Global.mPlayer.PlayNow(nextAlbumTracks.First());
                        setCurrentTrackInfo(nextAlbumTracks.First());
                        Global.AppActionMode = Global.AppActionModeEnum.Playing;
                    }
                    else
                    {
                        currentArtWorkIndex = 0;
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        public void _buttonRightArrow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentArtWorkIndex++;
                var nextAlbumTracks = AlbumTracks.Skip(10 * currentArtWorkIndex).Take(10).ToList();
                if (nextAlbumTracks.Any())
                {
                    this.LoadArtwork(AlbumLibrary, nextAlbumTracks);
                    Global.mPlayer.playList.PlayIndex = 0;
                    Global.mPlayer.SongPlayingArtWork = this;
                    Global.mPlayer.Play(nextAlbumTracks.First());
                    setCurrentTrackInfo(nextAlbumTracks.First());
                    Global.AppActionMode = Global.AppActionModeEnum.Playing;
                }
                else
                {
                    currentArtWorkIndex = 0;
                }
            }
            catch (Exception)
            {

            }
        }
    }
    public class RadioStation
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public ImageSource ImageSource { get; set; }
        public string Text { get; set; }
    }

}
