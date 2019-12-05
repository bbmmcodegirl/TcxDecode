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
        public ActivityViewModel ViewModel { get => DataContext as ActivityViewModel; }

        public ActivityView()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var property = typeof(ActivityViewModel).GetProperty(e.PropertyName);
            if (e.PropertyName.Contains("MetresS"))
            {
                e.Cancel = true;
            }
            if (e.PropertyName== "Dirty")
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
            if (e.PropertyName== nameof(Track.TrackPoints))
            {
                e.Cancel = true;
            }
        }

        private void Lap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
