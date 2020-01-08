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
using TcxDecode;

namespace TcxChart
{
    /// <summary>
    /// Interaction logic for ActivityView.xaml
    /// </summary>
    public partial class ActivityView : UserControl
    {
        private bool _multiDay = false;

        public ActivityViewModel ViewModel { get => DataContext as ActivityViewModel; }

        public ActivityView()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var property = typeof(LapViewModel).GetProperty(e.PropertyName);
            if (e.PropertyName.Contains("MetresS") || e.PropertyName == nameof(LapViewModel.TotalTimeSeconds) ||
                new string[] { nameof(LapViewModel.IsDirty), nameof(Lap.Intensity), nameof(Lap.TriggerMethod), nameof(LapViewModel.DistanceKm) }.Contains(e.PropertyName))
            {
                e.Cancel = true;
            }
            else if (e.PropertyName == nameof(LapViewModel.Duration))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:mm\:ss}";
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.DistanceMeters))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                e.Column.Header = "Distance";
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:n0} m";
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.AveragePace))
            {
                e.Column.Header = "Average Pace";
            }
            else if (e.PropertyName == nameof(LapViewModel.TargetSpeed))
            {
                e.Column.Header = "Target Speed";
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:n2} km/h";
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.TargetPace))
            {
                e.Column.Header = "Target Pace";
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    var binding = dataGridTextColumn.Binding as Binding;
                    if (binding == null)
                    {
                        dataGridTextColumn.Binding = new Binding(e.PropertyName) { Converter = new StringToPaceConverter(), Mode = BindingMode.TwoWay };
                    }
                    else
                    {
                        binding.Converter = new StringToPaceConverter();
                        binding.Mode = BindingMode.TwoWay;
                    }
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.AverageSpeedKmH))
            {
                e.Column.Header = "Average Speed";
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:n2} km/h";
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.AverageHeartRateBpm))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                e.Column.Header = "Avg Heart Rate";
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:0} bpm";
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.BestPace))
            {
                e.Column.Header = "Best Pace";
            }
            else if (e.PropertyName == nameof(LapViewModel.MaxSpeedKmH))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                e.Column.Header = "Max Speed";
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:n2} km/h";
                }
            }
            else if (e.PropertyName == nameof(LapViewModel.MaxHeartRateBpm))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                e.Column.Header = "Max Heart Rate";
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:0} bpm";
                }
            }
            else if (new string[] { nameof(LapViewModel.IsDirty), nameof(Lap.Intensity), nameof(Lap.TriggerMethod), nameof(LapViewModel.DistanceKm) }.Contains(e.PropertyName))
            {
                e.Cancel = true;
            }
            else if (property?.PropertyType == typeof(double) || property?.PropertyType == typeof(float))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = "{0:n2}";
                }
            }
            else if (property?.PropertyType == typeof(DateTime))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null && !_multiDay)
                {
                    dataGridTextColumn.Binding.StringFormat = @"{0:H\:mm\:ss}";
                }
            }
            if (e.PropertyName == nameof(Track.TrackPoints))
            {
                e.Cancel = true;
            }
        }

        private void Lap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                _multiDay = ViewModel.Laps.Select(l => l.StartTime.Date).Distinct().Count() > 1;
            }
        }

        private void LapsGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // TODO display a dialog and ask the user first for the distance to split
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) &&
                !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is DataGridRow)
            {
                DataGridRow row = dep as DataGridRow;
                var item = row.Item;
                var lap = item as LapViewModel;
                if (lap != null)
                {
                    this.ViewModel.SplitLap(lap, distanceMeters: 1000);
                }
            }
        }
    }
}
