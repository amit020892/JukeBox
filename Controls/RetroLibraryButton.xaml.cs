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

namespace JukeBoxSolutions
{
    /// <summary>
    /// Interaction logic for RetroLibraryButton.xaml
    /// </summary>
    public partial class RetroLibraryButton : UserControl
    {
     public string Content { get; set; }

        public RetroLibraryButton()
        {
            InitializeComponent();
            Content = "";
        }

        public RetroLibraryButton(string ContentText)
        {
            InitializeComponent();
            Content = ContentText;
        }

        private void btnSpotify_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
