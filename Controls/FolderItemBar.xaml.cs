using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for FolderItemBar.xaml
    /// </summary>
    public partial class FolderItemBar : UserControl
    {
        public bool Checked { get; set; }
        public System.Action<string> _setDirectory { get; set; }
        public FolderItemBar(string content, System.Action<string> setDirectory)
        {
            InitializeComponent();
            UnselectedfolderLabel.Content=SelectedFolderLabel.Content=content;
            _setDirectory=setDirectory;
        }

        public void Unchecked_Click(object sender, MouseButtonEventArgs e)
        {
            Checked = !Checked;
            UncheckedIcon.Visibility = Visibility.Collapsed;
            CheckedIcon.Visibility = Visibility.Visible;
            FolderBar.Background = Brushes.White;
            FolderBar.Opacity = 1;
            UnselectedfolderLabel.Visibility = Visibility.Collapsed;
            SelectedFolderLabel.Visibility = Visibility.Visible;
            SelectedFolderIcon.Visibility = Visibility.Visible;
            UnselectedFolderIcon.Visibility = Visibility.Collapsed;
            _setDirectory?.Invoke(SelectedFolderLabel.Content.ToString());
        }

        public void Checked_Click(object sender, MouseButtonEventArgs e)
        {
            Checked = !Checked;
            UncheckedIcon.Visibility = Visibility.Visible;
            CheckedIcon.Visibility = Visibility.Collapsed;
            FolderBar.Background = Brushes.Black;
            FolderBar.Opacity = 0.71;
            UnselectedfolderLabel.Visibility = Visibility.Visible;
            SelectedFolderLabel.Visibility = Visibility.Collapsed;
            SelectedFolderIcon.Visibility = Visibility.Collapsed;
            UnselectedFolderIcon.Visibility = Visibility.Visible;
        }
    }
}
