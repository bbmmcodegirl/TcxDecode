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
