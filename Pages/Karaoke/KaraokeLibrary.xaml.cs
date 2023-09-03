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

namespace JukeBoxSolutions.Pages.Karaoke
{
    /// <summary>
    /// Interaction logic for KaraokeLibrary.xaml
    /// </summary>
    public partial class KaraokeLibrary : Page
    {
        public KaraokeLibrary()
        {
            InitializeComponent();
        }

        internal async void GetKaraokeSongs()
        {
            try
            {
                using(JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                {
                    var songs = db.SongLibraries.Where(x => x.TrackLibraries.Any(c => c.Type.Contains(Global.AppModeString))).ToList();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
