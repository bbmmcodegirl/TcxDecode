using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TcxChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcxChartViewModel ViewModel { get => DataContext as TcxChartViewModel; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new TcxChartViewModel();

            sportFiltersPanel.Children.Clear();
            var sportsButtonStyle = this.Resources["SportButtonStyle"] as Style;
            foreach (var sportType in ViewModel.SportTypes)
            {
                var sportToggleButton = new System.Windows.Controls.Primitives.ToggleButton();
                sportToggleButton.Content = sportType;
                sportToggleButton.Style = sportsButtonStyle;
                sportToggleButton.Checked += SportToggleButton_Checked;
                sportToggleButton.Unchecked += SportToggleButton_Unchecked;
                sportToggleButton.IsChecked = true;
                sportFiltersPanel.Children.Add(sportToggleButton);
            }
        }

        CollectionView ActivitiesCollectionView { get => (CollectionView)CollectionViewSource.GetDefaultView(ActivitiesListView.ItemsSource); }

        private void ensureSportsFilter()
        {
            if (ActivitiesCollectionView != null && ActivitiesCollectionView.Filter == null)
            {
                ActivitiesCollectionView.Filter = ViewModel.SportFilter;
            }
        }

        private void SportToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ensureSportsFilter();
            var button = sender as ContentControl;
            var sport = button.Content.ToString();
            ViewModel.SportTypeSelected(sport);
            ActivitiesCollectionView?.Refresh();
        }

        private void SportToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ensureSportsFilter();
            var button = sender as ContentControl;
            var sport = button.Content.ToString();
            ViewModel.SportTypeUnselected(sport);
            ActivitiesCollectionView?.Refresh();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null)
            {
                foreach (var r  in e.RemovedItems)
                {
                    ViewModel.ActivityDeselected(r as ActivityViewModel);
                }

            }
            if (e.AddedItems != null)
            {
                foreach (var a in e.AddedItems)
                {
                    ViewModel.ActivitySelected(a as ActivityViewModel);
                }
            }
        }

        private void save_click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveActivitiesIfDirty();
        }
    }
}
