using System;
using System.Windows;
using System.Windows.Controls;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for StackYearControl.xaml
    /// </summary>
    public partial class StackYearControl : UserControl
    {
        Action<string> _onSelectYear { get; set; }
        Action<string> _onRemoveYear { get; set; }
        string yearLabel;
        public StackYearControl(string yearLabel,Action<string> onSelectYear,Action<string> onRemoveYear)
        {
            InitializeComponent();
            _onSelectYear = onSelectYear;
            _onRemoveYear = onRemoveYear;
            this.yearLabel = yearLabel;
            SelectedYearLabel.Content=UnselectedYearLabel.Content=yearLabel;
        }

        private void SelectYear(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                UnselectedYearLabelContainer.Visibility = Visibility.Collapsed;
                SelectedYearLabelContainer.Visibility = Visibility.Visible;
                _onSelectYear?.Invoke(yearLabel);
            }
            catch (Exception)
            {
            }
           
        }
        private void RemoveYear(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                UnselectedYearLabelContainer.Visibility = Visibility.Visible;
                SelectedYearLabelContainer.Visibility = Visibility.Collapsed;
                _onRemoveYear?.Invoke(yearLabel);
            }
            catch (Exception)
            {

            }
            
        }
    }
}
