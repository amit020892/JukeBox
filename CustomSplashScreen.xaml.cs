using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace JukeBoxSolutions
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class CustomSplashScreen : Window, INotifyPropertyChanged
    {
        private double _value1;
        public double Value1
        {
            get => _value1;
            set { _value1 = value; OnPropertyChanged(); }
        }
        public CustomSplashScreen()
        {
            InitializeComponent();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
            }
            catch (Exception)
            {
            }
        }
    }
}
