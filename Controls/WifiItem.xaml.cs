using ManagedNativeWifi;
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
    /// Interaction logic for WifiItem.xaml
    /// </summary>
    public partial class WifiItem : UserControl
    {
        private AvailableNetworkPack baseNetworkPack;
        private bool isConnected;

        public WifiItem(AvailableNetworkPack networkPack, bool isConnected = false)
        {
            InitializeComponent();
            this.baseNetworkPack = networkPack;
            this.isConnected = isConnected;
            if (isConnected)
            {
                baseButton.IsChecked = true;
                lblWifiName.Content = "-- " + networkPack.Ssid + " --";
                btnConnect.Visibility = Visibility.Collapsed;
                btnDisConnect.Visibility = Visibility.Visible;
            }
            else
            {
                lblWifiName.Content = networkPack.Ssid;
                btnConnect.Visibility = Visibility.Visible;
                btnDisConnect.Visibility = Visibility.Collapsed;
            }
        }

        private void btnPassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnForget_Click(object sender, RoutedEventArgs e)
        {
            NativeWifi.DeleteProfile(
        interfaceId: baseNetworkPack.Interface.Id,
        profileName: baseNetworkPack.ProfileName);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            NativeWifi.ConnectNetworkAsync(baseNetworkPack.Interface.Id, baseNetworkPack.ProfileName, baseNetworkPack.BssType, TimeSpan.FromSeconds(10));
        }

        private void btnDisConnect_Click(object sender, RoutedEventArgs e)
        {
            NativeWifi.DisconnectNetworkAsync(baseNetworkPack.Interface.Id, TimeSpan.FromSeconds(10));
        }

        private void btnExpand_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lblWifiName.Opacity = 0.2;
            lblOptions.Opacity = 0.2;
            stackOptions.Visibility = Visibility.Visible;
            btnPassword.Visibility = Visibility.Collapsed;
            tbPassword.Visibility = Visibility.Collapsed;
            btnForget.Visibility = Visibility.Visible;
        }


    }
}
