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
    /// Interaction logic for PopupNotifications.xaml
    /// </summary>
    public partial class PopupNotifications : UserControl
    {
        private int idCounter = 0;
        public PopupNotifications()
        {
            InitializeComponent();
        }

        private Label GetNewNotification(string text, int id)
        {
            Label l = new Label();
            l.Content = text;
            l.Tag = id;
            l.HorizontalAlignment = templateLabel.HorizontalAlignment;
            l.VerticalAlignment = templateLabel.VerticalAlignment;
            l.Padding = templateLabel.Padding;
            l.Foreground = templateLabel.Foreground;
            return l;
        }

        private void ClearOldNotifications()
        {

        }

        internal int NewNotification(string v)
        {
            if (v.StartsWith("iTunes API Calls"))
            {
                stackNotifications.Dispatcher.Invoke(new Action(() =>
                {
                    List<int> removeID = new List<int>();
                    foreach (var a in stackNotifications.Children)
                    {
                        if (((Label)a).Content.ToString().StartsWith("iTunes API Calls"))
                            removeID.Add(int.Parse(((Label)a).Tag.ToString()));
                    }

                    foreach (int r in removeID)
                        RemoveNotification(r);
                }));
            }

            idCounter++;
            int newId = idCounter;
            stackNotifications.Dispatcher.Invoke(new Action(() => { stackNotifications.Children.Add(GetNewNotification(v, newId)); }));
            return newId;
        }

        internal void UpdateNotification(int iNotification, string v)
        {
            stackNotifications.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var c in stackNotifications.Children.OfType<Label>())
                {
                    if ((int.Parse(c.Tag.ToString()) == iNotification))
                    {
                        c.Content = v;
                        break;
                    }
                }
            }));
        }

        internal void RemoveNotification(int iNotification)
        {
            stackNotifications.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var c in stackNotifications.Children.OfType<Label>())
                {
                    if ((int.Parse(c.Tag.ToString()) == iNotification))
                    {
                        stackNotifications.Children.Remove(c);
                        break;
                    }
                }
            }));
        }

    }
}
