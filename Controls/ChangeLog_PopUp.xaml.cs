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
using System.Reflection;
using System.Diagnostics;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for ChangeLog_PopUp.xaml
    /// </summary>
    public partial class ChangeLog_PopUp : UserControl
    {
        string version = "";
        public ChangeLog_PopUp()
        {
            InitializeComponent();
            lblTemplate.Visibility = Visibility.Collapsed;



            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            version = fileVersionInfo.ProductVersion;


            string v1_1_Header = "Version 0.9.1.x";
            v1_1_Header = "Version 1.2.5";

            List<string> v1_1 = new List<string>() {
                "Metadata: Import / Export Metadata"
                ,"Main Player: Pin track info while playing"
                ,"Main Player: Audio Visualizer"
                ,"Metadata: Auto Resume iTunes data download"
                ,"UI: Added Main Menu"
                ,"UI: Added Setting Page"
                ,"UI: Added Admin Page"
                ,"UI: Coloured Themes"
                ,"UI: Added Onscreen Keyboard"
                ,"UI: Added button description overlay"
                , "Library: Search Refinement Artist, Album, Song"
                , "Library: Added touch screen scroll (experimental)"

            };
            List<string> v1_2 = new List<string>() {
                "Log System 1.0"
                , " - Dynamic Log with relevant Data Dumps"
                , " - Log focus on Import and Notification Process"
                , "Various UI Bug Fixes"
                , "Adjustable size for on-screen Keyboard "
                , "Threaded 'Just Play' to prevent lag"
                , "Library Button Index set to Artists, and self cancels"
                , "Added offline import to import menu"
                , "Volume can now go up to 200%"
,"Added deadzone buffer to on-screen keyboard"
,"Added various exception handlers to prevent Album Art Crashes"
,"Added Library for Radio Mode"
,"---- Revision 2: ----"
,"Bug Fix: Crased on all Karaoke Songs -- Fixed"
,"Bug Fix: Crased on VLC Mode switch -- Fixed"
,"Bug Fix: Play All Album Songs (Library) -- Fixed"
,"Bug Fix: Play All Album Songs (Album) -- Fixed"
,"Log Expansion: Track Active VLC Core Player"
,"Additional Setting: Visualizer Quality & Off"
,"Rev. 3"
,"Added Multithreading to Import Manager"
,"Fixed Windows Not Responding When Importing"
,"Fixed Resume iTunes = Update Fuction"
,"Fixed Library Admin Unsorted Songs Filter"
,"Resolved Duplicate Songs issue"
,"Added track details feature"
,"Pending: Optimized Bulk Import"
            };
            List<string> v1_3 = new List<string>() {
            "Pre-Beta Test Build"
            };

            lblHeader.Content = v1_1_Header;
            foreach (string v in v1_3)
            {
                stackChanges.Children.Add(getLabel(v));
            }
        }

        private Label getLabel(string Text)
        {
            Label l = new Label();
            l.Content = Text;
            l.Foreground = lblTemplate.Foreground;
            l.FontFamily = lblTemplate.FontFamily;
            l.FontSize = lblTemplate.FontSize;
            return l;
        }
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Exit Control
            Global.mPlayer.TogglePopUp();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Viewed changes -- reset with each version
            Global.AdminSettings.StartupVersion = version;
            Global.mPlayer.TogglePopUp();
        }
    }
}
