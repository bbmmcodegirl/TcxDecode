using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            ViewModel.PropertyChanged += viewModel_PropertyChanged;
            displayChart();
        }

        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.StartsWith("DoShow"))
            {
                return;
            }

            displayChart();
        }

        private void displayChart()
        {
            int numPoints = (int)this.ActualWidth / 2;
            var xValues = ViewModel.GetTimeSeries(numPoints);

            var seriesCollection = new SeriesCollection();
            TimeChart.Series.Clear();

            if (ViewModel.DoShowSpeed)
            {
                var speeds = ViewModel.GetTimeLine<double>(nameof(TrackPoint.SpeedKmH), numPoints);
                var series = new LineSeries
                {
                    Title = "Speed",
                    Values = new ChartValues<double>(speeds),
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    LineSmoothness = 1,
                };
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowHeartRate)
            {
                var maxHeartRate = ViewModel.MaxHeartRate;
                var maxSpeed = ViewModel.MaxSpeed;
                var hrToSpeedFactor = maxSpeed / maxHeartRate;

                var heartRates = ViewModel.GetTimeLine<int>(nameof(TrackPoint.HeartRateBpm), numPoints).Select(hr => ((double)hr * hrToSpeedFactor)).ToList();
                var series = new LineSeries
                {
                    Title = "HeartRate",
                    Values = new ChartValues<double>(heartRates),
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    LineSmoothness = 0,
                    LabelPoint = chartPoint => $"{chartPoint.Y / hrToSpeedFactor}bpm",
                };
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowTargetSpeed)
            {
                var targetSpeeds = ViewModel.GetTimeLine<double>(nameof(LapViewModel.TargetSpeed), numPoints);
                seriesCollection.Add(new StepLineSeries
                {
                    Title = "Target Speed",
                    Fill = Brushes.Transparent,
                    Values = new ChartValues<double>(targetSpeeds),
                    PointGeometry = null,
                    AlternativeStroke = Brushes.Transparent,
                });
            }

            if (ViewModel.DoShowCadence)
            {
                var maxCadence = ViewModel.Laps.SelectMany(l => l.TrackPoints).Max(t => t.RunCadence);
                if (maxCadence > 0)
                {
                    var maxSpeed = ViewModel.MaxSpeed;
                    var cadenceToSpeedFactor = maxSpeed / maxCadence;

                    var cadences = ViewModel.GetTimeLine<double>(nameof(TrackPoint.RunCadence), numPoints).Select(cd => ((double)cd * cadenceToSpeedFactor)).ToList();
                    var series = new LineSeries
                    {
                        Title = "Cadence",
                        Values = new ChartValues<double>(cadences),
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        LineSmoothness = 0,
                        LabelPoint = chartPoint => $"{chartPoint.Y / cadenceToSpeedFactor} steps/min",
                    };
                    seriesCollection.Add(series);
                }
            }

            if (ViewModel.DoShowElevation)
            {
                var maxElevation = ViewModel.Laps.SelectMany(l => l.TrackPoints).Max(t => t.AltitudeMeters);
                if (maxElevation > 0)
                {
                    var maxSpeed = ViewModel.MaxSpeed;
                    var elevationToSpeedFactor = maxSpeed / maxElevation;

                    var elevations = ViewModel.GetTimeLine<double>(nameof(TrackPoint.AltitudeMeters), numPoints).Select(e => ((double)e * elevationToSpeedFactor)).ToList();
                    var series = new LineSeries
                    {
                        Title = "Elevation",
                        Values = new ChartValues<double>(elevations),
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        LineSmoothness = 0,
                        LabelPoint = chartPoint => $"{chartPoint.Y / elevationToSpeedFactor} m",
                    };
                    seriesCollection.Add(series);
                }
            }

            if (ViewModel.DoShowElevationChange && ViewModel.Laps.SelectMany(l => l.TrackPoints).Any())
            {
                var maxElevation = ViewModel.Laps.SelectMany(l => l.TrackPoints).Max(t => t.AltitudeMeters);
                var maxSpeed = ViewModel.MaxSpeed;
                if (maxElevation > 0 && maxSpeed > 0)
                {

                    var elevations = ViewModel.GetTimeLine<double>(nameof(TrackPoint.AltitudeMeters), numPoints).ToList();
                    var elevationChanges = new List<Double>();
                    elevationChanges.Add(0);
                    for (int i = 1; i < elevations.Count; i++)
                    {
                        elevationChanges.Add(elevations[i] - elevations[i - 1]);
                    }

                    var maxChange = elevationChanges.Max();
                    var minChange = elevationChanges.Min();
                    var totalChangeExtent = maxChange - minChange;
                    if (totalChangeExtent > 0)
                    {
                        var elevationChangeToSpeedFactor = maxSpeed / totalChangeExtent;
                        var series = new LineSeries
                        {
                            Title = "Elevation Change",
                            Values = new ChartValues<double>(elevationChanges),
                            PointGeometry = null,
                            Fill = Brushes.Transparent,
                            LineSmoothness = 0,
                            LabelPoint = chartPoint => $"{chartPoint.Y / elevationChangeToSpeedFactor} m",
                        };
                        seriesCollection.Add(series);
                    }
                }
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
