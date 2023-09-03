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

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for AdminSQL.xaml
    /// </summary>
    public partial class AdminSQL : Page
    {
        public AdminSQL()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            using(JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                //datagridv
                //var results = db.Database.SqlQuery<string>("Select * from LibraryView").ToList();
                //dataResults.ItemsSource = results;
            }
        }
    }
}
