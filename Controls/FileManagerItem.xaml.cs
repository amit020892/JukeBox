using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for FileManagerItem.xaml
    /// </summary>
    public partial class FileManagerItem : UserControl
    {
        private ImportFactory.ManagedFile mf { get; set; }
        ImportFactory.AnalysisFile aFile { get; set; }

        public FileManagerItem(ImportFactory.ManagedFile managedFile)
        {
            InitializeComponent();
            mf = managedFile;
            lblContentText.Content = managedFile.FullFileName;
            lblRawFileName.Content = managedFile.FullFileName;
            SetProperties();
        }

        public void SetProperties()
        {
            // check datatags
            stackActions.Children.Clear();

            if (mf.isDatabase)
            {
                lblContentText.Foreground = Brushes.Green;
                btnUpdate.Visibility = Visibility.Collapsed;
                btnImport.Visibility = Visibility.Collapsed;
                stackActions.Children.Add(new Label() { Content = "DB", Foreground = Brushes.Green });
            }
            else
            {
                if (mf.isImportReady)
                    stackActions.Children.Add(new Label() { Content = "Import Ready", Foreground = Brushes.Green });
                if (mf.hasId3)
                {
                    SetID3();
                }
            }

        }

        private void PullAnalysis()
        {
            string filename;
            bool pullartist = false;
            if (mf.Id3Tag != null)
            {
                if (mf.Id3Tag.Artists.Value.Count() == 1)
                    pullartist = true;
                else if (mf.Id3Tag.Artists.Value.Count() > 1)
                    pullartist = true;
            }

            aFile = Global.importFactory.AnalyseFilename(mf, pullartist);

            UpdateAnalysisControls();
        }

        private void UpdateAnalysisControls()
        {
            lblAlbum.Content = mf.searchAlbum;
            lblArtist.Content = mf.searchArtistCombined;
            lblTrack.Content = mf.searchTrack;
            if (mf.isImportReady || mf.isVideo)
                btnImport.IsEnabled = true;

            if (aFile.NamePortions.Count >= 1)
            {
                lblGrp1.Content = aFile.NamePortions[0].NamePortion;
                if (aFile.NamePortions[0].SlotMode != "Open")
                {
                    lblGrp1.Foreground = Brushes.Green;
                    btnSetArtistGrp1.IsEnabled = false;
                    btnSetTrackGrp1.IsEnabled = false;
                    btnSetTrackNoGrp1.IsEnabled = false;
                    btnCombineGrp1.IsEnabled = false;
                    btnDestroyGrp1.IsEnabled = false;

                    if (aFile.NamePortions[0].SlotMode == "Artist")
                    {
                        btnSetArtistGrp1.IsEnabled = true;
                        //lblGrp1.Content = mf.Id3Tag.Artists.Value.First();
                        //mf.searchArtist = mf.Id3Tag.Artists.Value.First();
                    }
                    if (aFile.NamePortions[0].SlotMode == "Track")
                        btnSetTrackGrp1.IsEnabled = true;
                    if (aFile.NamePortions[0].SlotMode == "TrackNumber")
                        btnSetTrackNoGrp1.IsEnabled = true;
                }
            }

            if (aFile.NamePortions.Count >= 2)
            {
                lblGrp2.Content = aFile.NamePortions[1].NamePortion;
                if (aFile.NamePortions[1].SlotMode != "Open")
                {
                    btnSetArtistGrp2.IsEnabled = false;
                    btnSetTrackGrp2.IsEnabled = false;
                    btnCombineGrp2.IsEnabled = false;
                    btnDestroyGrp2.IsEnabled = false;

                    if (aFile.NamePortions[1].SlotMode == "Artist")
                        btnSetArtistGrp2.IsEnabled = true;
                    if (aFile.NamePortions[1].SlotMode == "Track")
                    {
                        btnSetTrackGrp2.IsEnabled = true;
                        //mf.searchTrack = aFile.NamePortions[1].NamePortion;
                    }
                }
            }

            if (aFile.NamePortions.Count >= 3)
            {
                lblGrp3.Content = aFile.NamePortions[2].NamePortion;
                if (aFile.NamePortions[2].SlotMode != "Open")
                {
                    btnSetArtistGrp3.IsEnabled = false;
                    btnSetTrackGrp3.IsEnabled = false;
                    btnCombineGrp3.IsEnabled = false;
                    btnDestroyGrp3.IsEnabled = false;

                    if (aFile.NamePortions[2].SlotMode == "Artist")
                        btnSetArtistGrp3.IsEnabled = true;
                }
            }

            if (aFile.NamePortions.Count >= 4)
            {
                lblGrp4.Content = aFile.NamePortions[3].NamePortion;
                if (aFile.NamePortions[3].SlotMode != "Open")
                {
                    btnSetArtistGrp4.IsEnabled = false;
                    btnSetTrackGrp4.IsEnabled = false;
                    btnCombineGrp4.IsEnabled = false;
                    btnDestroyGrp4.IsEnabled = false;

                    if (aFile.NamePortions[3].SlotMode == "Artist")
                        btnSetArtistGrp4.IsEnabled = true;
                }
            }
        }

        private void UpdateAnalysisControls(ImportFactory.AnalysisFile aFile)
        {

        }

        public void SetID3()
        {
            bool isValid = true;

            //has Name
            //has Album
            //has Artist
            string name = "";
            string t = "";
            if (mf.Id3Tag == null)
            {

            }
            else
            {
                t += "Album: " + mf.Id3Tag.Album.Value;
                if (string.IsNullOrEmpty(mf.Id3Tag.Album.Value))
                    isValid = false;

                name = "";
                if (mf.Id3Tag.Artists.Value.Count() == 0)
                {
                    t += " | Artist(0): ";
                    isValid = false;
                }
                else
                {
                    t += " | Artist(" + mf.Id3Tag.Artists.Value.Count() + "): " + mf.Id3Tag.Artists.Value.First();
                    name += mf.Id3Tag.Artists.Value.First();
                    name += " - ";
                }

                if (string.IsNullOrEmpty(mf.Id3Tag.Title.Value))
                    isValid = false;

                t += " | Title: " + mf.Id3Tag.Title.Value;
                name += mf.Id3Tag.Title;

                this.ToolTip = t;

                if (isValid)
                {
                    lblContentText.Content = name;
                    stackActions.Children.Add(new Label() { Content = "ID3", Foreground = Brushes.Green });
                }
                else
                    stackActions.Children.Add(new Label() { Content = "ID3", Foreground = Brushes.Red });
            }
        }

        public bool isFirstToggle = true;
        public bool isOpen = false;
        private void lblContentText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isFirstToggle)
            {
                PullAnalysis();
                isFirstToggle = false;
            }

            if (isOpen)
            {
                Collapse();
                isOpen = false;
            }
            else
            {
                Expand();
                isOpen = true;
            }
        }

        public void Expand()
        {
            lblRawFileName.Visibility = Visibility.Visible;
            lblArtist.Visibility = Visibility.Visible;
            lblAlbum.Visibility = Visibility.Visible;
            lblTrack.Visibility = Visibility.Visible;

            stackGrp1.Visibility = Visibility.Visible;
            lblGrp1.Visibility = Visibility.Visible;
            if (btnSetArtistGrp1.IsEnabled) btnSetArtistGrp1.Visibility = Visibility.Visible;
            if (btnSetTrackGrp1.IsEnabled) btnSetTrackGrp1.Visibility = Visibility.Visible;
            if (btnSetTrackNoGrp1.IsEnabled) btnSetTrackNoGrp1.Visibility = Visibility.Visible;
            if (btnCombineGrp1.IsEnabled) btnCombineGrp1.Visibility = Visibility.Visible;
            if (btnDestroyGrp1.IsEnabled) btnDestroyGrp1.Visibility = Visibility.Visible;

            stackGrp2.Visibility = Visibility.Visible;
            lblGrp2.Visibility = Visibility.Visible;
            if (btnSetArtistGrp2.IsEnabled) btnSetArtistGrp2.Visibility = Visibility.Visible;
            if (btnSetTrackGrp2.IsEnabled) btnSetTrackGrp2.Visibility = Visibility.Visible;
            if (btnCombineGrp2.IsEnabled) btnCombineGrp2.Visibility = Visibility.Visible;
            if (btnDestroyGrp2.IsEnabled) btnDestroyGrp2.Visibility = Visibility.Visible;

            stackGrp3.Visibility = Visibility.Visible;
            lblGrp3.Visibility = Visibility.Visible;
            if (btnSetArtistGrp3.IsEnabled) btnSetArtistGrp3.Visibility = Visibility.Visible;
            if (btnSetTrackGrp3.IsEnabled) btnSetTrackGrp3.Visibility = Visibility.Visible;
            if (btnCombineGrp3.IsEnabled) btnCombineGrp3.Visibility = Visibility.Visible;
            if (btnDestroyGrp3.IsEnabled) btnDestroyGrp3.Visibility = Visibility.Visible;

            stackGrp4.Visibility = Visibility.Visible;
            lblGrp4.Visibility = Visibility.Visible;
            if (btnSetArtistGrp4.IsEnabled) btnSetArtistGrp4.Visibility = Visibility.Visible;
            if (btnSetTrackGrp4.IsEnabled) btnSetTrackGrp4.Visibility = Visibility.Visible;
            if (btnCombineGrp4.IsEnabled) btnCombineGrp4.Visibility = Visibility.Visible;
            if (btnDestroyGrp4.IsEnabled) btnDestroyGrp4.Visibility = Visibility.Visible;
        }

        public void Collapse()
        {
            lblRawFileName.Visibility = Visibility.Collapsed;
            lblArtist.Visibility = Visibility.Collapsed;
            lblAlbum.Visibility = Visibility.Collapsed;
            lblTrack.Visibility = Visibility.Collapsed;

            stackGrp1.Visibility = Visibility.Collapsed;
            lblGrp1.Visibility = Visibility.Collapsed;
            btnSetArtistGrp1.Visibility = Visibility.Collapsed;
            btnSetTrackGrp1.Visibility = Visibility.Collapsed;
            btnCombineGrp1.Visibility = Visibility.Collapsed;
            btnDestroyGrp1.Visibility = Visibility.Collapsed;

            stackGrp2.Visibility = Visibility.Collapsed;
            lblGrp2.Visibility = Visibility.Collapsed;
            btnSetArtistGrp2.Visibility = Visibility.Collapsed;
            btnSetTrackGrp2.Visibility = Visibility.Collapsed;
            btnCombineGrp2.Visibility = Visibility.Collapsed;
            btnDestroyGrp2.Visibility = Visibility.Collapsed;

            stackGrp3.Visibility = Visibility.Collapsed;
            lblGrp3.Visibility = Visibility.Collapsed;
            btnSetArtistGrp3.Visibility = Visibility.Collapsed;
            btnSetTrackGrp3.Visibility = Visibility.Collapsed;
            btnCombineGrp3.Visibility = Visibility.Collapsed;
            btnDestroyGrp3.Visibility = Visibility.Collapsed;

            stackGrp4.Visibility = Visibility.Collapsed;
            lblGrp4.Visibility = Visibility.Collapsed;
            btnSetArtistGrp4.Visibility = Visibility.Collapsed;
            btnSetTrackGrp4.Visibility = Visibility.Collapsed;
            btnCombineGrp4.Visibility = Visibility.Collapsed;
            btnDestroyGrp4.Visibility = Visibility.Collapsed;
        }

        private void btnSetArtistGrp_Click(object sender, RoutedEventArgs e)
        {
            // Add Artist to Buffer
            string destroy = "DOOM!";

            for (int i = 1; i <= 4; i++)
            {
                if (((Button)sender).Name.Contains("Grp" + i))
                {
                    aFile.NamePortions[i - 1].SlotMode = "Artist";
                    aFile.hasArtist = true;
                    UpdateAnalysisControls();
                }
            }
            // Update other Tracks
        }

        private void btnSetTrackGrp_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= 4; i++)
            {
                if (((Button)sender).Name.Contains("Grp" + i))
                {
                    aFile.NamePortions[i - 1].SlotMode = "Track";
                    aFile.hasTrack = true;
                    UpdateAnalysisControls();
                }
            }
        }

        private void btnSetTrackNoGrp_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= 4; i++)
            {
                if (((Button)sender).Name.Contains("Grp" + i))
                {
                    aFile.NamePortions[i - 1].SlotMode = "TrackNumber";
                    aFile.hasTrackNumber = true;
                    UpdateAnalysisControls();
                }
            }
        }

        private void btnDestroyGrp_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i <= 4; i++)
            {
                if (((Button)sender).Name.Contains("Grp" + i))
                {
                    aFile.NamePortions.RemoveAt(i - 1);
                    UpdateAnalysisControls();
                }
            }
        }

        private void btnCombineGrp_Click(object sender, RoutedEventArgs e)
        {
            string destroy = "DOOM!";

            for (int i = 1; i <= 4; i++)
            {
                if (((Button)sender).Name.Contains("Grp" + i))
                {
                    //aFile.NamePortions[i-1].SlotMode = "Track";
                    //UpdateAnalysisControls();
                }
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            // Connect to MusicBrainz and PullData
            // Save Data to Managed File
            // Update Track Label
            // Unlock Save DB

            if (string.IsNullOrEmpty(mf.searchAlbum) && aFile.hasAlbum)
            {
                mf.searchAlbum = aFile.NamePortions.First(xs => xs.SlotMode == "Album").NamePortion.Trim();
            }
            //if (string.IsNullOrEmpty(mf.searchArtist) && aFile.hasArtist)
            //{
            //    mf.searchArtist = aFile.NamePortions.First(xs => xs.SlotMode == "Artist").NamePortion.Trim();
            //}
            if (string.IsNullOrEmpty(mf.searchTrack) && aFile.hasTrack)
            {
                mf.searchTrack = aFile.NamePortions.First(xs => xs.SlotMode == "Track").NamePortion.Trim();
            }

            if (false)
            {
                var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var client = new MusicBrainzClient()
                {
                    Cache = new FileRequestCache(System.IO.Path.Combine(location, "cache"))
                };

                OnlineFactory.RunSongSearch(client);
            }
            //OnlineFactory2.RunSongSearch(client, song, "Amanda Palmer");
            //OnlineFactory online = new OnlineFactory();
            //OnlineFactory.RunTrackSearch(client, "The Thing About Things");

            // pull working file data
            // get possible search names
            //foreach (ManagedFile inbox in Inbox)

            //ManagedFile inbox = Inbox.First();
            //ImportFactory.WorkbenchFile WorkingFile = new ImportFactory.WorkbenchFile(mf);

            // search existing albums

            string s = "Something went wrong";
            //isSimple = false;
            //isComplex = false;
            //TrackSearchResults = new List<MusicBrainzHelper>();
            btnImport.IsEnabled = true;

        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Update ID3 Tag
            // Save to DB
            // has all 3
            Artist a = new Artist();
            if (mf.isVideo)
            {
                Global.importFactory.ImportVideoFile(mf);
                }
            else if (mf.isImportReady)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    Global.importFactory.ImportMusicFile(mf, db);

                    this.Visibility = Visibility.Collapsed;
                }
            }
        }



        private void lblGrp1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            lblGrp1.Visibility = Visibility.Collapsed;
            tbGrp1.Text = lblGrp1.Content.ToString();
            tbGrp1.Visibility = Visibility.Visible;
            tbGrp1.Focus();
        }

        private void tbGrp1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbGrp1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbGrp1.Text != "")
            {
                aFile.NamePortions[0].NamePortion = tbGrp1.Text;
                lblGrp1.Visibility = Visibility.Visible;
                tbGrp1.Visibility = Visibility.Collapsed;
                lblGrp1.Focus();
                UpdateAnalysisControls();
            }
        }
    }
}
