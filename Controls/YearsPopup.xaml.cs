using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for YearsPopup.xaml
    /// </summary>
    public partial class YearsPopup : UserControl
    {
        public Action<string> AddYearFilter { get; set; }
        public Action<string> RemoveYearFilter { get; set; }

        public YearsPopup(List<string> list)
        {
            InitializeComponent();
        }

        public YearsPopup(List<string> list, Action<string> addYearFilter, Action<string> removeYearFilter) : this(list)
        {
            InitializeComponent();
            CreateYearStacks(list);
            AddYearFilter = addYearFilter;
            RemoveYearFilter = removeYearFilter;
        }

        private void CreateYearStacks(List<string> list)
        {            
            foreach (string s in list)
            {
                var x = new StackYearControl(s, AddYearFilter,RemoveYearFilter);
                x.Margin = new Thickness(0, 5, 0, 5);
                YearStackPanel.Children.Add(x);
            }
        }

        private void BtnCancelPopup(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
            Global.mPlayer.popUp_Frame.Content = null;
            Library.IsLoading = false;
        }
        private void BtnCreateStack(object sender, RoutedEventArgs e)
        {
            Library.IsLoading = false;
        }

        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
            Global.mPlayer.popUp_Frame.Content = null;
            Library.IsLoading = false;
        }
    }
}
