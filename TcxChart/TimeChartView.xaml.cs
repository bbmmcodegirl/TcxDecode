using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using TcxDecode;

namespace TcxChart
{
    public partial class TimeChartView : UserControl
    {
        ActivityViewModel ViewModel { get; set; }

        public TimeChartView()
        {
            InitializeComponent();
        }
 
        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ViewModel = DataContext as ActivityViewModel;
            if (ViewModel == null)
            {
                return;
            }

            // TODO make us a proper viewModel and bind to that.
            int numPoints = (int)this.ActualWidth / 2;
            var maxHeartRate = ViewModel.MaxHeartRate;
            var maxSpeed = ViewModel.MaxSpeed;
            var hrToSpeedFactor = maxSpeed / maxHeartRate;

            var speeds = ViewModel.GetTimeLine<double>(nameof(TrackPoint.SpeedKmH), numPoints);
            var heartRates = ViewModel.GetTimeLine<int>(nameof(TrackPoint.HeartRateBpm), numPoints).Select(hr => ((double)hr*hrToSpeedFactor)).ToList();
            var targetSpeeds = ViewModel.GetTimeLine<double>(nameof(LapViewModel.TargetSpeed), numPoints);
            var xValues = ViewModel.GetTimeSeries(numPoints);

            TimeChart.Series.Clear();
            var seriesCollection = new SeriesCollection();
            seriesCollection.Add(new LineSeries
            {
                Title = "Speed",
                Values = new ChartValues<double>(speeds),
                PointGeometry = null,
                Fill = Brushes.Transparent,
                LineSmoothness = 1,
            });
            seriesCollection.Add(new LineSeries
            {
                Title = "HeartRate",
                Values = new ChartValues<double>(heartRates),
                PointGeometry = null,
                Fill = Brushes.Transparent,
                LineSmoothness = 0,
                LabelPoint = chartPoint => $"{chartPoint.Y / hrToSpeedFactor}bpm",
            });

            if (targetSpeeds.Any(t => t != 0))
            {
                seriesCollection.Add(new StepLineSeries
                {
                    Title = "Target Speed",
                    Fill = Brushes.Transparent,
                    Values = new ChartValues<double>(targetSpeeds),
                    PointGeometry = null,
                    AlternativeStroke = Brushes.Transparent,
                });
            }

            TimeChart.Series.AddRange(seriesCollection);

            TimeChart.AxisY.Clear();
            var speedAxis = new Axis()
            {
                LabelFormatter = value => value.ToString()
            };
            TimeChart.AxisY.Add(speedAxis); // TODO try to have multiple different axes for speed and heartrate

            TimeChart.AxisX.Clear();
            var timeAxis = new Axis()
            {
                Labels = xValues.Select(b => b.ToString()).ToArray()
            };
            TimeChart.AxisX.Add(timeAxis); 

            TimeChart.Zoom = ZoomingOptions.X;
        }
    }
}
