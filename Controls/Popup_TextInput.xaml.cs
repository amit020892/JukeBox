using JukeBoxSolutions.Pages;
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
    /// Interaction logic for Popup_TextInput.xaml
    /// </summary>
    public partial class Popup_TextInput : UserControl
    {
        public enum ControlMode
        {
            RenamePlaylist,
            NewPlaylist,
            Search,
            TextEdit
        }
        private string instructionMessage { get; set; }
        private ControlMode popupMode { get; set; }
        private Library ParentLibrary { get; set; }
        private Page parentPage { get; set; }
        private PlayListDetail basePlaylistDetail { get; set; }
        private int basePlaylistID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlMode"></param>
        /// <param name="TextEdit"></param>
        /// <param name="InstructionMessage">Enter text...</param>
        public Popup_TextInput(ControlMode controlMode, string TextEdit, string InstructionMessage = "")
        {
            //if (Global.AdminSettings.ShowKeyboard)
            //    Application.Current.Resources["TouchButtonHeight"] = Global.AdminSettings.KeyboardButtonSize;
            InitializeComponent();
            popupMode = controlMode;
            txtInput.Text = TextEdit;
            instructionMessage = InstructionMessage;
            setup();
        }
        public Popup_TextInput(ControlMode controlMode, int PlaylistID)
        {
            //if (Global.AdminSettings.ShowKeyboard)
            //    Application.Current.Resources["TouchButtonHeight"] = Global.AdminSettings.KeyboardButtonSize;
            InitializeComponent();
            popupMode = controlMode;
            basePlaylistID = PlaylistID;
            setup();
        }
        public Popup_TextInput(ControlMode controlMode)
        {
            //if (Global.AdminSettings.ShowKeyboard)
            //    Application.Current.Resources["TouchButtonHeight"] = Global.AdminSettings.KeyboardButtonSize;
            InitializeComponent();
            popupMode = controlMode;
            //parentPage = page;
            setup();
        }

        public Popup_TextInput(ControlMode controlMode, Library library)
        {
            InitializeComponent();
            popupMode = controlMode;
            ParentLibrary = library;
            setup();
        }

        private void setup()
        {
            if (Global.AdminSettings.ShowKeyboard)
            {
                gridKeyBoard.Visibility = Visibility.Visible;
            }
            else
                gridKeyBoard.Visibility = Visibility.Collapsed;


            if (popupMode == ControlMode.TextEdit)
            {
                if (string.IsNullOrEmpty(instructionMessage))
                    lblInstruction.Content = "Enter text...";
                else
                    lblInstruction.Content = instructionMessage;
            }
            else if (popupMode == ControlMode.Search)
            {
                // activate search buttons.. of sorts
                lblInstruction.Content = "Search...";
                txtInput.Text = "";
                gridSearchChoices.Visibility = Visibility.Visible;
                toggleSeachChoiceAll.IsChecked = true;
            }
            else
            {
                if (popupMode == ControlMode.RenamePlaylist)
                    lblInstruction.Content = "Rename playlist...";
                txtInput.Text = "";
            }

            txtInput.Focus();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


            // Exit Control
            Global.mPlayer.TogglePopUp();
        }

        private void btOkay_Click(object sender, RoutedEventArgs e)
        {
            if (popupMode == ControlMode.TextEdit)
            {
                Global.mPlayer.PopUpTextBuffer = txtInput.Text;
                Global.mPlayer.TogglePopUp();
            }
            else
            {


                Type t = Global.MainFrame.Content.GetType();

                if (Global.mPlayer.hasPopUpBackPocket)
                {
                    t = Global.mPlayer.PopUpBackPocket.GetType();
                }

                string s = t.Name.ToString();

                bool isFail = false;
                switch (s)
                {
                    case "PlayListView":
                        if (popupMode == ControlMode.NewPlaylist)
                        {
                            // Create and Set New Playlist
                            CreateNewPlaylist();
                            ((PlayListView)Global.mPlayer.PopUpBackPocket).SetPlaylist(txtInput.Text, Global.AppPlaylistModeBufferID);
                        }
                        break;
                    case "FileManagerV3":
                        ((FileManagerV3)Global.MainFrame.Content).SetPlaylist(txtInput.Text);
                        break;
                    case "Library":
                        if (popupMode == ControlMode.Search)
                            ((Library)Global.MainFrame.Content).SetSearchFilter(txtInput.Text, bufferSearchMode);
                        else if (popupMode == ControlMode.NewPlaylist)
                        {
                            // Create and Set New Playlist
                            CreateNewPlaylist(true);
                            ((Library)Global.MainFrame.Content).SetPlaylistMode(basePlaylistDetail, Library.PlaylistModeEnum.NewPlaylist);
                        }
                        else if (popupMode == ControlMode.RenamePlaylist)
                        {
                            if (RenamePlaylist() == true)
                            {
                                ((Library)Global.MainFrame.Content).ReloadLibrary();
                            }
                            else
                                isFail = true;
                        }
                        break;
                }

                if (isFail == false)
                    Global.mPlayer.TogglePopUp();
                else
                    setup();
                // Exit Control
            }
        }

        private bool RenamePlaylist()
        {
            string playlistName = txtInput.Text.Trim();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // check if name exists
                var p = db.PlayListDetails.Where(w => playlistName.ToLower() == w.Name.ToLower());

                if (p.Any())
                {
                    lblError.Content = "Playlist name already exists";
                    return false;
                }
                else
                {
                    // get original playlist
                    var pDetail = db.PlayListDetails.Find(basePlaylistID);

                    // rename playlist
                    pDetail.Name = playlistName;

                    // save changes
                    db.Entry(pDetail).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
        }
        private void CreateNewPlaylist(bool useAppModeString = false)
        {
            string playlistName = txtInput.Text;
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var p = db.PlayListDetails.Where(w => playlistName.ToLower() == w.Name.ToLower());

                if (p.Any())
                {
                    string error = "A playlist by that name already exist";
                    Global.mPlayer.ShowNotification("Playlist already exists, selecting Playlist");

                    Global.AppControlMode = Global.AppControlModeEnum.Playlist;
                    Global.AppPlaylistModeBufferID = p.First().Id;
                    basePlaylistDetail = p.First();
                }
                else
                {
                    PlayListDetail newPlaylist = new PlayListDetail() { Name = playlistName, Type = 1 };
                    Global.mPlayer.playList.CopyTypeSettings(ref newPlaylist);
                    if (useAppModeString)
                        switch (Global.AppMode)
                        {
                            case Global.AppModeEnum.Karaoke:
                                newPlaylist.isKaraoke = true;
                                break;
                            case Global.AppModeEnum.Video:
                                newPlaylist.isVideo = true;
                                break;
                            case Global.AppModeEnum.Music:
                                newPlaylist.isMusic = true;
                                break;
                            case Global.AppModeEnum.Radio:
                                newPlaylist.isRadio = true;
                                break;
                        }

                    db.PlayListDetails.Add(newPlaylist);
                    db.SaveChanges();

                    // Global.mPlayer.SetNewPlaylist();

                    Global.AppControlMode = Global.AppControlModeEnum.Playlist;
                    Global.AppPlaylistModeBufferID = newPlaylist.Id;
                    basePlaylistDetail = newPlaylist;
                }
            }


        }

        private void btnKey_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            txtInput.Text += b.Content.ToString();
        }

        private void btnSpace_Click(object sender, RoutedEventArgs e)
        {
            txtInput.Text += " ";
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            string text = txtInput.Text;
            if (text.Length > 0)
                txtInput.Text = text.Substring(0, text.Length - 1);
        }


        internal Library.SearchMode bufferSearchMode = Library.SearchMode.All;
        private void toggleSeachChoiceAll_Checked(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.All;
            toggleSeachChoiceArtist.IsChecked = false;
            toggleSeachChoiceAlbum.IsChecked = false;
            toggleSeachChoiceSong.IsChecked = false;
        }

        private void toggleSeachChoiceArtist_Checked(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.Artist;
            toggleSeachChoiceAll.IsChecked = false;
            toggleSeachChoiceAlbum.IsChecked = false;
            toggleSeachChoiceSong.IsChecked = false;
        }

        private void toggleSeachChoiceAlbum_Checked(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.Album;
            toggleSeachChoiceAll.IsChecked = false;
            toggleSeachChoiceArtist.IsChecked = false;
            toggleSeachChoiceSong.IsChecked = false;
        }

        private void toggleSeachChoiceSong_Checked(object sender, RoutedEventArgs e)
        {
            bufferSearchMode = Library.SearchMode.Song;
            toggleSeachChoiceAll.IsChecked = false;
            toggleSeachChoiceArtist.IsChecked = false;
            toggleSeachChoiceAlbum.IsChecked = false;
        }


        private void UnloadAllChoices()
        {
            toggleSeachChoiceAll.IsChecked = false;
            toggleSeachChoiceArtist.IsChecked = false;
            toggleSeachChoiceAlbum.IsChecked = false;
            toggleSeachChoiceSong.IsChecked = false;
        }
    }
}
