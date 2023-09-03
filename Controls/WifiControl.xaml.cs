using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for WifiControl.xaml
    /// </summary>
    public partial class WifiControl : UserControl
    {
        public WifiControl()
        {
            InitializeComponent();
            var c = NativeWifi.EnumerateAvailableNetworks();
            var a = NativeWifi.EnumerateConnectedNetworkSsids().Select(y => y.ToString());
            var n = NativeWifi.EnumerateAvailableNetworkSsids().Select(x => x.ToString()); // UTF-8 string representation

            foreach (var n1 in c)
            {
                if (a.Contains(n1.Ssid.ToString()))
                    stackWifi.Children.Add(new WifiItem(n1, true));
                else
                    stackWifi.Children.Add(new WifiItem(n1));

            }
        }

    }
}
