using JukeBoxSolutions.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for FileManagerV3.xaml
    /// </summary>
    public partial class FileManagerV3 : Page
    {

        #region Import All

        public bool isBusyLoading
        {
            get { return gridLoading.Visibility == Visibility.Visible; }
            set
            {
                if (value == true) gridLoading.Dispatcher.Invoke(() => { gridLoading.Visibility = Visibility.Visible; });
                else
                {
                    gridLoading.Dispatcher.Invoke(() => { gridLoading.Visibility = Visibility.Collapsed; });
                    LoadDirectories(basePath, isUsingSubfolders);
                }
            }
        }

        // USED TO MAKE SURE NO CHANGES MADE!
        //private bool _isLocked = false;
        private bool _isPleasewait = false;
        internal bool isPleaseWait
        {
            get { return _isPleasewait; }
            set
            {
                _isPleasewait = value;
                if (value == true) gridPleaseWait.Visibility = Visibility.Visible;
                else gridPleaseWait.Visibility = Visibility.Collapsed;
            }
        }


        protected bool isUsingSubfolders = false;
        protected bool isCopyToDefaultFolder = false;
        public void ImportAll()
        {
            isBusyLoading = true;

            Global.LastImportDirectory = basePath;
            bool isMusicChecked = btnOptionMusic.IsChecked.Value;
            bool isKaraokeChecked = btnOptionKaraoke.IsChecked.Value;
            bool isVideoChecked = btnOptionVideo.IsChecked.Value;

            //ThreadPool.QueueUserWorkItem(_ => ImportAllAsync(isMusicChecked, isKaraokeChecked, isVideoChecked));

            ImportAllAsync(isMusicChecked, isKaraokeChecked, isVideoChecked);
            //Global.importFactory.LoadDirlisting(basePath, isUsingSubfolders);

            // Playlist Name!
            //if (btnOptionMusic.IsChecked.Value)
            //{
            //    isBusyLoading = Global.importFactory.ImportAllMusicFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
            //}
            //if (btnOptionKaraoke.IsChecked.Value)
            //{
            //    isBusyLoading = Global.importFactory.ImportAllKaraokeFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
            //}
            //if (btnOptionVideo.IsChecked.Value)
            //{
            //    isBusyLoading = Global.importFactory.ImportAllVideoFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
            //}

            //UpdateInterfaceAsync();
        }

        private async void ImportAllAsync(bool isMusicChecked, bool isKaraokeChecked, bool isVideoChecked)
        {
            try
            {
                btnContinueImporting.Dispatcher.Invoke(() => { btnContinueImporting.IsEnabled = false; });
                lblLoadingPleaseWaitAction.Dispatcher.Invoke(() => { lblLoadingPleaseWaitAction.Content = "Reading files from drive"; });
                Global.importFactory.LoadDirlisting(basePath, isUsingSubfolders);


                lblLoadingPleaseWaitAction.Dispatcher.Invoke(() => { lblLoadingPleaseWaitAction.Content = "Importing files to library"; });
                btnContinueImporting.Dispatcher.Invoke(() => { btnContinueImporting.IsEnabled = true; });
                // Playlist Name!
                if (isMusicChecked)
                {
                    var result = Global.importFactory.ImportAllMusicFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
                }
                if (isKaraokeChecked)
                {
                    Global.importFactory.ImportAllKaraokeFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
                }
                if (isVideoChecked)
                {
                    Global.importFactory.ImportAllVideoFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
                }

                UpdateInterfaceAsync();
            }
            catch (Exception ex)
            {

            }
        }


        #endregion Import All

        #region Import Selected


        List<string> FullImportedFolders = new List<string>();
        List<string> DataBaseFiles = new List<string>();
        List<string> SelectedFiles = new List<string>();
        List<string> ImportingFiles = new List<string>();


        public void ImportSelected()
        {
            try
            {
                isBusyLoading = true;
                btnContinueImporting.Dispatcher.Invoke(() => { btnContinueImporting.IsEnabled = false; });

                Global.LastImportDirectory = basePath;
                lblLoadingPleaseWaitAction.Dispatcher.Invoke(() => { lblLoadingPleaseWaitAction.Content = "Reading files from drive"; });
                Global.importFactory.LoadDirFiles(SelectedFiles);

                ImportingFiles = SelectedFiles;
                SelectedFiles.Clear();

                lblLoadingPleaseWaitAction.Dispatcher.Invoke(() => { lblLoadingPleaseWaitAction.Content = "Importing files to library"; });
                btnContinueImporting.Dispatcher.Invoke(() => { btnContinueImporting.IsEnabled = true; });
                if (btnOptionMusic.IsChecked.Value)
                    Global.importFactory.ImportAllMusicFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
                if (btnOptionKaraoke.IsChecked.Value)
                    Global.importFactory.ImportAllKaraokeFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");
                if (btnOptionVideo.IsChecked.Value)
                    Global.importFactory.ImportAllVideoFiles(isCopyToDefaultFolder, hasPlaylistName == true ? NewPlaylistName : "");

                UpdateInterfaceAsync();
            }
            catch (Exception)
            {

            }

        }


        #endregion Import Selected

        public enum ImportMode
        {
            USBMode,
            FileMode,
            CDRipMode,
            OnlineMode
        }

        private ImportMode SelectedImportMode { get; set; }
        private string importDirectory { get; set; }


        public FileManagerV3(ImportMode importMode = ImportMode.FileMode, string startingDir = "")
        {
            Global.importFactory.DisableImportValidation = false;
            string debugCrash = "";
            InitializeComponent();
            try
            {
                //debugCrash = "Controlled Crash";
                //int i = int.Parse("xxx");
                SelectedImportMode = importMode;

                debugCrash = "Set Import Directory";
                if (Global.DefaultImportDirectory == "")
                    importDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\MyJukebox";
                else
                    importDirectory = Global.DefaultImportDirectory;

                debugCrash = "Set Base Path";
                if (SelectedImportMode == ImportMode.USBMode && startingDir != "")
                {
                    basePath = startingDir;
                    btnCopyToDrive.Visibility = Visibility.Visible;
                    isCopyToDefaultFolder = true;
                }
                else
                    basePath = Global.LastImportDirectory;

                debugCrash = "Set Breadcrumbs";
                SetBreadCrumbs(basePath);
                debugCrash = "Load Directories";
                LoadDirectories(basePath);
                debugCrash = "Load Index";
                LoadIndex();
            }
            catch
            {
                // Something went wrong!
                gridLoading.Visibility = Visibility.Visible;
                lblLoadingPleaseWait.Visibility = Visibility.Collapsed;
                diagnosticStack.Visibility = Visibility.Visible;

                Label crashStep = newDebugLabel();
                crashStep.Content = "Crash Step: " + debugCrash;
                diagnosticStack.Children.Add(crashStep);



                Label l0 = newDebugLabel();
                l0.Content = "Import Mode: " + importMode.ToString();
                diagnosticStack.Children.Add(l0);

                Label l1 = newDebugLabel();
                l1.Content = "Import Dir: " + importDirectory;
                diagnosticStack.Children.Add(l1);

                Label l2 = newDebugLabel();
                l2.Content = "Starting Dir: " + startingDir;
                diagnosticStack.Children.Add(l2);

                Label l3 = newDebugLabel();
                l3.Content = "Default Import Dir: " + Global.DefaultImportDirectory;
                diagnosticStack.Children.Add(l3);

                Label l4 = newDebugLabel();
                l4.Content = "Last Import Dir: " + Global.LastImportDirectory;
                diagnosticStack.Children.Add(l4);

                Label l5 = newDebugLabel();
                l5.Content = "Base Path: " + basePath;
                diagnosticStack.Children.Add(l5);
            }
            Global.MainWindow.MenuMinimalToggle(Visibility.Collapsed);
        }

        #region USB Copy

        private Label newDebugLabel()
        {
            Label baseLabel = new Label();
            baseLabel.Foreground = lblCrash.Foreground;
            baseLabel.FontFamily = lblCrash.FontFamily;
            baseLabel.FontSize = lblCrash.FontSize;
            return baseLabel;
        }

        public void FindDrives()
        {
        }


        #endregion USB Copy

        List<string> breadcrumbs = new List<string>();
        private string _basePath = @"C:\";
        string basePath { get { return _basePath; } set { _basePath = value.Replace("\\\\", "\\"); } }
        private bool isMusicSelected = true;
        private bool isKaraokeSelected = true;
        private bool isVideoSelected = true;
        private int MusicCount
        {
            get { return _musicCount; }
            set
            {
                _musicCount = value;
                if (value == 0)
                {
                    btnOptionMusic.Dispatcher.Invoke(() =>
                    {
                        btnOptionMusic.Background = Brushes.Black;
                        btnOptionMusic.Foreground = Brushes.White;
                        btnOptionMusic.IsChecked = false;
                    });
                    BtnMusicCount.Dispatcher.Invoke(() => { BtnMusicCount.Content = value; });
                }
                else
                {
                    btnOptionMusic.Dispatcher.Invoke(() =>
                    {
                        btnOptionMusic.Background = Brushes.White;
                        btnOptionMusic.Foreground = Brushes.Black;
                        btnOptionMusic.IsChecked = true;
                    });
                    if (value == 1)
                        BtnMusicCount.Dispatcher.Invoke(() => { BtnMusicCount.Content = value; });
                    else
                        BtnMusicCount.Dispatcher.Invoke(() => { BtnMusicCount.Content = value; });
                }
            }
        }
        private int _musicCount = 0;

        private int VideoCount
        {
            get { return _videoCount; }
            set
            {
                _videoCount = value;
                if (value == 0)
                {
                    btnOptionVideo.Dispatcher.Invoke(() =>
                    {
                        btnOptionVideo.Background = Brushes.Black;
                        btnOptionVideo.Foreground = Brushes.White;
                        btnOptionVideo.IsChecked = false;
                    });
                    BtnVideoCount.Dispatcher.Invoke(() => { BtnVideoCount.Content = value; });
                }
                else
                {
                    btnOptionVideo.Dispatcher.Invoke(() =>
                    {
                        btnOptionVideo.Background = Brushes.White;
                        btnOptionVideo.Foreground = Brushes.Black;
                        btnOptionVideo.IsChecked = true;
                    });
                    if (value == 1)
                        BtnVideoCount.Dispatcher.Invoke(() => { BtnVideoCount.Content = value; });
                    else
                        BtnVideoCount.Dispatcher.Invoke(() => { BtnVideoCount.Content = value; });
                }
            }
        }
        private int _videoCount = 0;

        //  "0 Music Tracks" Gri
        //  "0 Video Tracks" Gri
        //  t="0 Karaoke Tracks"
        //
        //

        private int KaraokeCount
        {
            get { return _karaokeCount; }
            set
            {
                _karaokeCount = value;
                if (value == 0)
                {
                    btnOptionKaraoke.Dispatcher.Invoke(() =>
                    {
                        btnOptionKaraoke.Background = Brushes.Black;
                        btnOptionKaraoke.Foreground = Brushes.White;
                        btnOptionKaraoke.IsChecked = false;
                    });
                    BtnKaraokeCount.Dispatcher.Invoke(() => { BtnKaraokeCount.Content = value; });
                }
                else
                {
                    btnOptionKaraoke.Dispatcher.Invoke(() =>
                    {
                        btnOptionKaraoke.Background = Brushes.White;
                        btnOptionKaraoke.Foreground = Brushes.Black;
                        btnOptionKaraoke.IsChecked = true;
                    });
                    if (value == 1)
                        BtnKaraokeCount.Dispatcher.Invoke(() => { BtnKaraokeCount.Content = value; });
                    else
                        BtnKaraokeCount.Dispatcher.Invoke(() => { BtnKaraokeCount.Content = value; });
                }
            }
        }
        private int _karaokeCount = 0;


        protected void SetBreadCrumbs(string pathSelection)
        {
            if (string.IsNullOrEmpty(pathSelection))
            {
                return;
            }

            btnImport.IsEnabled = false;
            btnImport.Background = Brushes.Black;
            btnImport.Foreground = Brushes.White;
            isFiles = false;
            // build from scratch
            stackBreadCrumbs.Children.Clear();
            var d = new DirectoryInfo(pathSelection);
            if (pathSelection == "ChooseDrive" || new DirectoryInfo(pathSelection).Root.ToString().Replace(@"\", "") == pathSelection)
            {
                Button r = getNewBreadcrumb("Change Drive", "ChooseDrive");
                stackBreadCrumbs.Children.Add(r);
            }

            if (pathSelection != "ChooseDrive")
            {
                if (pathSelection.Contains(@"\") || pathSelection.Contains(":"))
                {
                    breadcrumbs.Clear();
                    foreach (string s in pathSelection.Split('\\'))
                    {
                        if (!string.IsNullOrEmpty(s))
                            breadcrumbs.Add(s);
                    }
                    basePath = pathSelection;
                }
                else
                {
                    breadcrumbs.Add(pathSelection);
                    basePath = basePath + "\\" + pathSelection;
                    //stackBreadCrumbs.Children.Add(getNewBreadcrumb(pathSelection, basePath));
                }

                string navPath = "";
                foreach (string s2 in breadcrumbs)
                {
                    navPath += s2;
                    string text = s2.Length > 10 ? s2.Substring(0, 10) + ".." : s2;
                    stackBreadCrumbs.Children.Add(getNewBreadcrumb(text, navPath));
                    navPath += "\\";
                }
            }
        }

        protected string getSearchPattern()
        {
            string pattern = "";
            pattern = "*.mp3|*.mp4";
            if (!isMusicSelected)
                foreach (string s in Global.musicformats)
                {
                    pattern += "|*" + s;
                }

            if (!isKaraokeSelected)
                foreach (string s in Global.karaokeformats)
                {
                    pattern += "|*" + s;
                }

            if (!isVideoSelected)
                foreach (string s in Global.videoformats)
                {
                    pattern += "|*" + s;
                }

            if (pattern.StartsWith("|"))
                pattern = pattern.Substring(1);

            return pattern;
        }

        private List<string> GetValidFiles(string[] dirFiles)
        {
            List<string> f = new List<string>();
            var x = from s in dirFiles select new FileInfo(s);
            var x2 = from s2 in x
                     where Global.karaokeformats.Contains(s2.Extension.ToLower()) ||
                     Global.musicformats.Contains(s2.Extension.ToLower()) ||
                Global.videoformats.Contains(s2.Extension.ToLower())
                     select s2;

            MusicCount = (from iM in x2
                          where Global.musicformats.Contains(iM.Extension.ToLower())
                          select iM).Count();

            KaraokeCount = (from iK in x2
                            where Global.karaokeformats.Contains(iK.Extension.ToLower())
                            select iK).Count();

            VideoCount = (from iV in x2
                          where Global.videoformats.Contains(iV.Extension.ToLower())
                          select iV).Count();

            f = (from x3 in x2 select x3.FullName).ToList();
            return f;
        }

        int filecount = 0;
        int foldercount = 0;

        internal void AuditLoadDirectoriesAsync(string path, bool includeSubFolders = false)
        {
            ThreadPool.QueueUserWorkItem(_ => AuditLoadDirectories(path, includeSubFolders));
        }
        protected void AuditLoadDirectories(string path, bool includeSubFolders = false)
        {
            List<string> dir = new List<string>();

            //Add dash at end of root directory
            //path = path.Replace("\\\\", "\\");
            if (!path.EndsWith(@"\"))
                path = path + @"\";

            //check if dir is in file listings
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                //      where i.FilePath == (path + "\\" + i.FileName + i.Extention)

                if (isUsingSubfolders)
                {
                    var x = from i in db.TrackLibraries
                            where i.FilePath.StartsWith(path)
                            select i.FilePath;

                    DataBaseFiles = x.ToList();
                }
                else
                {
                    var xA = from i in db.TrackLibraries
                                 //where i.FilePath == (path + "\\" + i.FileName + i.Extention)
                             where i.FilePath == (path + i.FileName + i.Extention)
                             select i.FilePath;

                    DataBaseFiles = xA.ToList();
                }
            }

            if (includeSubFolders)
                dir = GetValidFiles(Directory.GetFiles(path, "*", SearchOption.AllDirectories)).ToList();
            else
                dir = GetValidFiles(Directory.GetFiles(path)).ToList();

            dir.RemoveAll(r => DataBaseFiles.Contains(r));

            foldercount = Directory.GetDirectories(path).Count();

            DirPage = 0;
            btnNext.Dispatcher.Invoke(() => { btnNext.IsEnabled = dir.Count() > PageCount; });
            btnNext.Dispatcher.Invoke(() => { btnPrevious.IsEnabled = false; });

            dir.RemoveAll(r => DataBaseFiles.Contains(r));
            MainDirListing = dir;
            _CoreDirListing = dir;
            PopulateDirectories();
            LoadIndex();
        }

        internal void LoadDirectoriesAsync(string path, bool includeSubFolders = false)
        {
            ThreadPool.QueueUserWorkItem(_ => LoadDirectories(path, includeSubFolders));
        }
        protected void LoadDirectories(string path, bool includeSubFolders = false)
        {
            List<string> dir = new List<string>();
            filecount = 0;
            foldercount = 0;

            if (path == "ChooseDrive")
            {
                List<DriveInfo> drives = new List<DriveInfo>();

                switch (SelectedImportMode)
                {
                    case ImportMode.USBMode:
                        drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable).ToList();
                        break;
                    case ImportMode.FileMode:
                        drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Fixed).ToList();
                        break;
                }

                if (drives.Count == 0)
                {

                }
                else
                {
                    dir = (from d in drives select d.ToString()).ToList();
                }

            }
            else
            {
                //Add dash at end of root directory
                if (!path.EndsWith(@"\"))
                    path = path + @"\";


                //check if dir is in file listings
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    //      where i.FilePath == (path + "\\" + i.FileName + i.Extention)

                    if (isUsingSubfolders)
                    {
                        var x = from i in db.TrackLibraries
                                where i.FilePath.StartsWith(path)
                                select i.FilePath;

                        DataBaseFiles = x.ToList();
                    }
                    else
                    {
                        var xA = from i in db.TrackLibraries
                                     //where i.FilePath == (path + "\\" + i.FileName + i.Extention)
                                 where i.FilePath == (path + i.FileName + i.Extention)
                                 select i.FilePath;

                        DataBaseFiles = xA.ToList();
                    }
                }

                if (isFiles)
                {
                    if (includeSubFolders)
                        dir = GetValidFiles(Directory.GetFiles(path, "*", SearchOption.AllDirectories)).ToList();
                    else
                        dir = GetValidFiles(Directory.GetFiles(path)).ToList();

                    foldercount = Directory.GetDirectories(path).Count();
                }
                else
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    DirectoryInfo[] files = directory.GetDirectories();

                    var filtered = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden) && !f.Attributes.HasFlag(FileAttributes.System)).ToList();
                    dir = (from f in filtered select f.FullName).ToList();
                    //dir = Directory.GetDirectories(path).Where(w => !w.EndsWith("System Volume Information") && !w.Contains("$RECYCLE.BIN")).ToList();


                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        FullImportedFolders = new List<string>();
                        List<string> tempDirectories = new List<string>();

                        foreach (var d in dir)
                        {
                            //where i.FilePath == (path + "\\" + i.FileName + i.Extention)
                            if (db.TrackLibraries.Where(i => (i.FilePath == (d + "\\" + i.FileName + i.Extention))).Any())
                            {
                                tempDirectories.Add(d);
                            }
                        }

                        foreach (var e in tempDirectories)
                        {
                            var subFiles = GetValidFiles(Directory.GetFiles(e)).Count();
                            var dbFiles = db.TrackLibraries.Where(i => (i.FilePath == (e + "\\" + i.FileName + i.Extention))).Count();

                            if (subFiles == dbFiles)
                                FullImportedFolders.Add(e);
                        }
                    }


                    if (includeSubFolders)
                    {
                        try
                        {
                            filecount = GetValidFiles(Directory.GetFiles(path, "*", SearchOption.AllDirectories)).Count();
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            filecount = GetValidFiles(Directory.GetFiles(path)).Count();
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }


            DirPage = 0;
            btnNext.Dispatcher.Invoke(() => { btnNext.IsEnabled = dir.Count() > PageCount; });
            btnNext.Dispatcher.Invoke(() => { btnPrevious.IsEnabled = false; });

            dir.RemoveAll(r => DataBaseFiles.Contains(r));
            MainDirListing = dir;
            _CoreDirListing = dir;
            PopulateDirectories();
            LoadIndex();


        }


        internal void UpdateInterfaceAsync()
        {
            ThreadPool.QueueUserWorkItem(_ => PopulateDirectories());
        }

        protected void PopulateDirectories()
        {
            stackFiles.Dispatcher.Invoke(() =>
            {
                stackFiles.Children.Clear();

                if (isFiles)
                {
                    Button f = new Button();
                    if (foldercount > 0)
                    {
                        f.Content = foldercount == 1 ? foldercount.ToString() + " folder" : foldercount.ToString() + " folders";
                        f.Style = FindResource(Global.ControlStyle_FolderButton) as Style;
                        f.Click += new RoutedEventHandler(btnSwitchFolders_Click);
                        stackFiles.Children.Add(f);
                    }
                }

                List<string> dir = new List<string>();
                int dirIndex = PageCount * DirPage;
                for (int i = dirIndex; i < (PageCount * (DirPage + 1)); i++)
                {
                    if (i < MainDirListing.Count())
                        dir.Add(MainDirListing[i]);
                }

                foreach (string s in dir)
                {
                    if (isFiles)
                    {
                        ToggleButton b = new ToggleButton();
                        b.Content = System.IO.Path.GetFileName(s);
                        b.Tag = s;

                        b.Style = FindResource(Global.ControlStyle_FileListingToggle) as Style;

                        if (Global.importFactory.ImportQue.ContainsDir(s))
                        {
                            //b.Background = btnTemplateGreenRedHighlight.BorderBrush;
                            b.IsChecked = true;
                            b.Click += AlwaysChecked;
                            b.IsEnabled = true;
                        }
                        else
                        {
                            b.Click += new RoutedEventHandler(SelectFile);
                        }

                        if (DataBaseFiles.Contains(s))
                            b.IsEnabled = false;
                        if (SelectedFiles.Contains(s))
                            b.IsChecked = true;

                        stackFiles.Children.Add(b);
                    }
                    else
                    {
                        //Button b = new Button();
                        //if (System.IO.Path.GetPathRoot(s) == s)
                        //    b.Content = System.IO.Path.GetPathRoot(s);
                        //else
                        //    b.Content = System.IO.Path.GetFileName(s);

                        //b.Tag = s;

                        //b.Style = FindResource(Global.ControlStyle_FolderButton) as Style;
                        //b.Click += new RoutedEventHandler(SetDirectory);

                        //if (FullImportedFolders.Contains(s))
                        //{
                        //    b.Opacity = 0.5;
                        //}

                        FolderItemBar folderItemBar;
                        if (System.IO.Path.GetPathRoot(s) == s)
                            folderItemBar = new FolderItemBar(System.IO.Path.GetPathRoot(s), SetSelectedFolderPath);
                        else
                            folderItemBar = new FolderItemBar(System.IO.Path.GetFileName(s), SetSelectedFolderPath);

                        folderItemBar.Margin = new Thickness(0, 10, 0, 0);
                        BtnSelect.Click -= new RoutedEventHandler(SetDirectoryPath);
                        BtnSelect.Click += new RoutedEventHandler(SetDirectoryPath);
                        //if (!isUsingSubfolders)
                        //{
                        //    folderItemBar.Checked_Click(null, null);
                        //}
                        //else
                        //{
                        //    folderItemBar.Unchecked_Click(null, null);
                        //}
                        stackFiles.Children.Add(folderItemBar);
                    }
                }



                if (!isFiles)
                {
                    FolderItemBar folderItemBar;
                    //Button f = new Button();
                    if (filecount > 0)
                    {
                        folderItemBar = new FolderItemBar(filecount == 1 ? filecount.ToString() + " file in folder" : filecount.ToString() + " files in folder", btnSwitchFiles_Click);
                        //f.Content = filecount == 1 ? filecount.ToString() + " file in folder" : filecount.ToString() + " files in folder";
                        //f.Click += new RoutedEventHandler(btnSwitchFiles_Click);
                        //f.Style = FindResource(Global.ControlStyle_FolderButton) as Style;
                        folderItemBar.Margin = new Thickness(0, 10, 0, 0);
                        stackFiles.Children.Add(folderItemBar);
                        BtnSelect.Click -= btnSwitchFiles_Click;
                        BtnSelect.Click += btnSwitchFiles_Click;
                    }
                    else
                        stackFiles.Children.Add(new Label() { Content = "No usable files in Folder" });
                }


                isPleaseWait = false;
                btnCheckSubFolders.IsEnabled = true;
                //btnCheckSubFolders.Background = Brushes.White;
                //btnCheckSubFolders.Foreground = Brushes.Black;
                if (filecount >0)
                {                    
                    btnImport.IsEnabled = true;
                    btnImport.Background = Brushes.White;
                    btnImport.Foreground = Brushes.Black;
                    btnToggleServerVerification.IsChecked = Global.importFactory.DisableImportValidation;
                }
                

            });
        }

        private string selectedFolderPath { get; set; }
        private void SetSelectedFolderPath(string obj)
        {
            selectedFolderPath = obj;
        }

        private void AlwaysChecked(object sender, RoutedEventArgs e)
        {
            ((ToggleButton)sender).IsChecked = true;
        }

        protected void SelectFile(object sender, EventArgs e)
        {
            // change button
            ToggleButton b = ((ToggleButton)sender);
            b.IsChecked = true;
            b.IsEnabled = true;
            b.Background = Brushes.White;
            b.Foreground = Brushes.Black;
            string s = b.Tag.ToString();
            if (b.IsChecked.GetValueOrDefault())
            {
                btnImport.Content = "Import Selected";
                SelectedFiles.Add(s);
            }
            else
            {
                SelectedFiles.Remove(s);
                if (!SelectedFiles.Any())
                    btnImport.Content = "Import All";
            }
        }

        protected void SetDirectory(object sender, EventArgs e)
        {
            string path = ((Button)sender).Content.ToString();
            SetBreadCrumbs(path);
            LoadDirectories(basePath, isUsingSubfolders);
        }
        public void SetDirectoryPath(object sender, EventArgs e)
        {
            SetBreadCrumbs(selectedFolderPath);
            LoadDirectories(basePath, isUsingSubfolders);
            selectedFolderPath = null;
        }

        #region FilterIndex


        protected void LoadIndex()
        {

            Global.MainWindow.sPanel.Dispatcher.Invoke(() =>
            {
                Global.MainWindow.btnKeyboard.Visibility = Visibility.Visible;
                Global.MainWindow.sParentPanel.Visibility = Visibility.Visible;
                Global.MainWindow.btnKeyboard.Visibility = Visibility.Collapsed;
                Global.MainWindow.btnMediaPanel.Visibility = Visibility.Visible;
                Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Collapsed;
                Global.MainWindow.sPanel.Children.Clear();

                //Button t = btnTemplate;
                //Button buttHash = GetNewIndexButton('#');
                ToggleButton buttHash = GetDisabledNewIndexButton('#');

                buttHash.IsEnabled = true;
                for (int number = 0; number <= 9; number++)
                {
                    if (MainDirListing.Any(a => a.Split('\\').Last().StartsWith(number.ToString(), true, CultureInfo.CurrentCulture)))
                    {
                        buttHash.IsEnabled = true;
                        buttHash.Style = FindResource("EnabledToggleButtonStyle") as Style;
                        break;
                    }
                }

                Global.MainWindow.sPanel.Children.Add(buttHash);

                for (char letter = 'A'; letter <= 'Z'; letter++)
                {
                    //b.Content = letter;
                    //ToggleButton newB = GetNewIndexButton(letter);

                    //newB.IsEnabled = false;
                    //if (MainDirListing.Any(a => a.Split('\\').Last().StartsWith(letter.ToString(), true, CultureInfo.CurrentCulture)))
                    //    newB.IsEnabled = true;

                    ToggleButton newB;
                    if (MainDirListing.Any(a => a.Split('\\').Last().StartsWith(letter.ToString(), true, CultureInfo.CurrentCulture)))
                    {
                        newB = GetEnabledNewIndexButton(letter);
                        newB.IsEnabled = true;
                    }
                    else
                    {
                        newB = GetDisabledNewIndexButton(letter);
                        newB.IsEnabled = false;
                    }


                    Global.MainWindow.sPanel.Children.Add(newB);
                }
            });
        }


        string btnToggle = "";
        ToggleButton btnToggleActual { get; set; }
        private void btnIndex_Click(object sender, RoutedEventArgs e)
        {
            DirPage = 0;
            string a = ((ToggleButton)sender).Content.ToString();
            if (btnToggle == a)
            {
                // reset filter
                MainDirListing = _CoreDirListing;
                btnToggle = "";

                DirPage = 0;
                PopulateDirectories();
            }
            else
            {
                //foreach (string s in pathSelection.Split('\\'))


                MainDirListing = (from t in _CoreDirListing
                                  where t.Split('\\').Last().ToUpper().StartsWith(a)
                                  select t).ToList();

                if (btnToggleActual != null)
                    btnToggleActual.IsChecked = false;
                btnToggleActual = ((ToggleButton)sender);
                btnToggle = a;

                DirPage = 0;
                PopulateDirectories();
            }


            //BuildCards();
        }
        public ToggleButton GetNewIndexButton(char content)
        {
            ToggleButton b = new ToggleButton();
            //Button b = new Button();
            b.Content = content;
            b.Click += new RoutedEventHandler(btnIndex_Click);
            b.Padding = new Thickness(0, 0, 0, 8);
            b.Margin = new Thickness(2, 0, 2, 0);
            b.Height = 81;
            b.MinWidth = 40;
            b.Width = 40;
            b.FontSize = 18;

            b.Style = FindResource(Global.ControlStyle_ToggleButton) as Style;
            //b.Style = FindResource(Global.ControlStyle_Button) as Style;
            b.IsEnabled = false;

            return b;
        }

        public ToggleButton GetEnabledNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Style = FindResource("EnabledToggleButtonStyle") as Style;
            b.IsEnabled = false;

            return b;
        }

        public ToggleButton GetDisabledNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Style = FindResource("DisabledToggleButtonStyle") as Style;
            b.IsEnabled = false;

            return b;
        }


        #endregion FilterIndex
        protected Button getNewBreadcrumb(string Text, string navPath)
        {
            Button b2 = getBreadcrumbRoot(Text, Text.Contains(":"));
            //b2.Style = FindResource(Global.ControlStyle_Button) as Style;
            //b2.Height = 80;
            //b2.FontSize = 18;
            //b2.Visibility = Visibility.Visible;
            b2.Click += new RoutedEventHandler(NavBreadcrumb);
            b2.Tag = navPath;
            //b2.Width = double.NaN;
            //b2.MinWidth = 0;
            b2.Margin = new Thickness(5, 0, 0, 0);
            //b2.Content = Text;
            return b2;
        }
        protected Button getBreadcrumbRoot(string Text, bool isRoot = false)
        {
            Button b2 = new Button();
            if (isRoot)
            {
                b2.Style = FindResource(Global.ControlStyle_InitialPathButton) as Style;
            }
            else
            {
                b2.Style = FindResource(Global.ControlStyle_FolderPathButton) as Style;
            }

            b2.Visibility = Visibility.Visible;
            //b2.Click += new RoutedEventHandler(NavBreadcrumb);
            ////b2.Tag = navPath;
            //b2.Width = double.NaN;
            //b2.MinWidth = 0;
            //b2.Margin = new Thickness(3, 0, 3, 0);
            b2.Content = Text;
            return b2;
        }

        protected void NavBreadcrumb(object sender, EventArgs e)
        {
            string sPath = ((Button)sender).Tag.ToString();
            isUsingSubfolders = false;
            btnCheckSubFolders.IsChecked = false;

            if (sPath == "ChooseDrive")
            {
                isFiles = false;
                SetBreadCrumbs("ChooseDrive");
                LoadDirectories("ChooseDrive");
            }
            else
            {
                SetBreadCrumbs(sPath);
                LoadDirectories(sPath);
            }
        }

        protected string getLastDir()
        {
            return @"Z:\Vids\Music Vids";
        }

        private void btnSwitchFolders_Click(object sender, RoutedEventArgs e)
        {
            isFiles = false;
            UncheckedIcon.Visibility = Visibility.Visible;
            CheckedIcon.Visibility = Visibility.Visible;
            btnSwitchFolders.IsChecked = true;
            btnSwitchFolders.Background = Brushes.White;
            btnSwitchFolders.Foreground = Brushes.Black;
            btnSwitchFiles.IsChecked = false;
            btnSwitchFiles.Background = Brushes.Black;
            btnSwitchFiles.Foreground = Brushes.White;
            btnImport.IsEnabled = false;
            btnImport.Background = Brushes.Black;
            btnImport.Foreground = Brushes.White;
            LoadDirectories(basePath);
        }

        private void btnSwitchFiles_Click(object sender, RoutedEventArgs e)
        {
            isFiles = true;
            UncheckedIcon.Visibility = Visibility.Collapsed;
            CheckedIcon.Visibility = Visibility.Collapsed;
            btnSwitchFolders.IsChecked = false;
            btnSwitchFiles.IsChecked = true;
            btnSwitchFolders.Background = Brushes.Black;
            btnSwitchFolders.Foreground = Brushes.White;
            btnSwitchFiles.Background = Brushes.White;
            btnSwitchFiles.Foreground = Brushes.Black;
            LoadDirectories(basePath);
        }

        private void btnSwitchFiles_Click(string path)
        {
            btnImport.IsEnabled = true;
            btnImport.Background = Brushes.White;
            btnImport.Foreground = Brushes.Black;
        }

        private bool isFiles
        {
            get { return _isFiles; }
            set
            {
                if (_isFiles != value)
                {
                    // initiate swap
                    var bBuffer = btnSwitchFiles.Background;
                    var fBuffer = btnSwitchFiles.Foreground;

                    btnSwitchFiles.Background = btnSwitchFolders.Background;
                    btnSwitchFiles.Foreground = btnSwitchFolders.Foreground;

                    btnSwitchFolders.Background = bBuffer;
                    btnSwitchFolders.Foreground = fBuffer;

                    _isFiles = value;
                }
            }
        }

        private bool _isFiles = false;

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    btnImport.IsEnabled = false;
                    // initiate import process
                    if (SelectedFiles.Any())
                        ImportSelected();
                    else
                        ImportAll();

                    Global.MainWindow.KaraokeLibraryInstancesLoaded = false;
                    Global.MainWindow.MusicLibraryInstancesLoaded = false;
                    Global.MainWindow.RadioLibraryInstancesLoaded = false;
                    Global.MainWindow.VideoLibraryInstancesLoaded = false;
                    Global.MainWindow.CurrentLibraryInstnace = null;
                    Global.MainWindow.CurrentKaraokeLibraryInstnace = null;
                    Global.MainWindow.CurrentRadioLibraryInstnace = null;
                    Global.MainWindow.CurrentVideoLibraryInstnace = null;
                });

            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// The number of records to load per page
        /// </summary>
        protected int PageCount = SystemParameters.PrimaryScreenWidth == 1920 ? 10 : 6;
        private List<string> MainDirListing = new List<string>();
        private List<string> _CoreDirListing = new List<string>();

        /// <summary>
        /// The main directory page, also controls the side buttons
        /// </summary>
        private int DirPage
        {
            get { return _dirPageIndex; }
            set
            {
                if (_dirPageIndex >= 0 && _dirPageIndex < MainDirListing.Count())
                {
                    _dirPageIndex = value;
                    btnPrevious.Dispatcher.Invoke(() => { btnPrevious.IsEnabled = _dirPageIndex > 0; });
                    //13-- 1 * 12 = 12; 2 * 12 = 24
                    btnNext.Dispatcher.Invoke(() => { btnNext.IsEnabled = MainDirListing.Count() > ((_dirPageIndex + 1) * PageCount); });
                }
            }
        }

        private int _dirPageIndex = 0;

        #region Events

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            DirPage++;
            PopulateDirectories();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            DirPage--;
            PopulateDirectories();
        }

        private void btnCheckSubFolders_Click(object sender, RoutedEventArgs e)
        {
            UIHelpers.SetButtonBusy(ref btnCheckSubFolders, this);
            btnCheckSubFolders.IsEnabled = false;
            isPleaseWait = true;

            isUsingSubfolders = btnCheckSubFolders.IsChecked.GetValueOrDefault();
            if (!isUsingSubfolders)
            {
                btnImport.IsEnabled=false;
                btnImport.Background = Brushes.Black;
                btnImport.Foreground = Brushes.White;
            }
            LoadDirectoriesAsync(basePath, isUsingSubfolders);
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (btnSelectAll.Content.ToString() == "SELECT ALL")
            {
                SelectedFiles.AddRange(MainDirListing.Where(w => !SelectedFiles.Contains(w)));
                btnSelectAll.Content = "DESELECT ALL";
            }
            else
            {
                SelectedFiles.Clear();
                btnSelectAll.Content = "SELECT ALL";
            }
            PopulateDirectories();
        }

        private void btnAddToPlaylist_Checked(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.PopUp_NewPlaylist();
            Global.mPlayer.TogglePopUp();
        }


        #endregion Events

        private void page_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private string NewPlaylistName { get; set; }
        private bool hasPlaylistName
        {
            get
            {
                return _hasPlaylistName;
            }
            set
            {
                if (value == true)
                {
                    btnPlayListName.Visibility = Visibility.Visible;
                    btnPlayListName.Content = "x " + NewPlaylistName;
                }
                else
                {
                    btnPlayListName.Visibility = Visibility.Collapsed;
                    btnPlayListName.Content = "";
                    btnAddToPlaylist.IsChecked = false;
                }

                _hasPlaylistName = value;
            }
        }

        private bool _hasPlaylistName { get; set; }

        public void SetBusyOff()
        {
            isBusyLoading = false;
        }
        public void SetPlaylist(string playListName)
        {
            NewPlaylistName = playListName;
            hasPlaylistName = true;
        }

        private void btnPlayListName_Click(object sender, RoutedEventArgs e)
        {
            NewPlaylistName = "";
            hasPlaylistName = false;
        }

        private void btnContinueImporting_Click(object sender, RoutedEventArgs e)
        {
            isBusyLoading = false;
        }

        private void btnToggleServerVerification_Click(object sender, RoutedEventArgs e)
        {
            if (btnToggleServerVerification.IsChecked.Value)
                Global.importFactory.DisableImportValidation = true;
            else
                Global.importFactory.DisableImportValidation = false;

        }

        private void btnAuditImport_Checked(object sender, RoutedEventArgs e)
        {
            // Swap out music items for whatever isn't in the db
            //Global.importFactory.
            AuditLoadDirectoriesAsync(basePath, isUsingSubfolders);
            PopulateDirectories();

        }

        private void btnAuditImport_Unchecked(object sender, RoutedEventArgs e)
        {
            // Swap back to normal browsing
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new SearchPopup();
        }

        private void CheckedAllIcon_Click(object sender, MouseButtonEventArgs e)
        {
            CheckedIcon.Visibility = Visibility.Collapsed;
            UncheckedIcon.Visibility = Visibility.Visible;
        }

        private void UncheckedAllIcon_Click(object sender, MouseButtonEventArgs e)
        {
            CheckedIcon.Visibility = Visibility.Visible;
            UncheckedIcon.Visibility = Visibility.Collapsed;
        }
    }

    public class FileFolderContent
    {
        public string Content { get; set; }
        public string Tag { get; set; }
    }
}
