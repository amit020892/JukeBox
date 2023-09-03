using System.Windows.Controls;
namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryLabel.xaml
    /// </summary>
    public partial class SongGroupLabelContainer : UserControl
    {
        public SongGroupLabelContainer(string title, int count)
        {
            InitializeComponent();
            lblTitle.Content = title;
            BtnMusicCount.Content = count;
        }
    }
}
