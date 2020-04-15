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

            var maxHeartRate = ViewModel.MaxHeartRate;
            var maxSpeed = ViewModel.MaxSpeedKmH;
            var maxCadence = ViewModel.MaxCadence;
            var maxElevation = ViewModel.MaxElevation;
            var maxStrideLength = ViewModel.MaxStrideLengthM;

            TimeChart.AxisY.Clear();

            bool showAnySpeed = ViewModel.DoShowSpeed || ViewModel.DoShowTargetSpeed;
            bool showAnyHeartRate = ViewModel.DoShowHeartRate;
            bool showAnyPace = ViewModel.DoShowPace || ViewModel.DoShowTargetPace;
            bool showAnyElevation = ViewModel.DoShowElevation || ViewModel.DoShowElevationChange;
            bool showAnyCadence = ViewModel.DoShowCadence;
            bool showAnyStrideLength = ViewModel.DoShowStrideLength;

            int speedAxisIndex = -1;
            int heartRateAxisIndex = -1;
            int paceAxisIndex = -1;
            int elevationAxisIndex = -1;
            int cadenceAxisIndex = -1;
            int strideLengthAxisIndex = -1;
            if (showAnySpeed)
            {
                var speedAxis = new Axis()
                {
                    Title = "Speed",
                    Foreground = System.Windows.Media.Brushes.DodgerBlue,
                    LabelFormatter = value =>
                    {
                        return value.ToString("0.0");
                    }
                };
                speedAxisIndex = TimeChart.AxisY.Count;
                TimeChart.AxisY.Add(speedAxis);
            }
            if (showAnyHeartRate)
            {
                var heartRateAxis = new Axis()
                {
                    Title = "HeartRate (bpm)",
                    Foreground = System.Windows.Media.Brushes.IndianRed,
                    LabelFormatter = value =>
                    {
                        return value.ToString("0");
                    },
                    Position=AxisPosition.RightTop
                };
                heartRateAxisIndex = TimeChart.AxisY.Count;
                TimeChart.AxisY.Add(heartRateAxis);
            }
            if (showAnyPace)
            {
                var paceAxis = new Axis()
                {
                    Title = "Pace (min/km)",
                    Foreground = System.Windows.Media.Brushes.DarkOliveGreen,
                    LabelFormatter = value => new Pace(value).ToString(),
                };
                paceAxisIndex = TimeChart.AxisY.Count;
                TimeChart.AxisY.Add(paceAxis);
            }
            if (showAnyElevation)
            {
                var elevationAxis = new Axis()
                {
                    Title = "Elevation (m)",
                    Foreground = System.Windows.Media.Brushes.Black,
                    LabelFormatter = value => value.ToString("0"),
                    Position = AxisPosition.RightTop
                };
                elevationAxisIndex = TimeChart.AxisY.Count;
                TimeChart.AxisY.Add(elevationAxis);
            }
            if (showAnyCadence)
            {
                var cadenceAxis = new Axis()
                {
                    Title = "Cadence (steps/min)",
                    Foreground = System.Windows.Media.Brushes.Magenta,
                    LabelFormatter = value => value.ToString("0"),
                    Position = AxisPosition.RightTop
                };
                cadenceAxisIndex = TimeChart.AxisY.Count;
                TimeChart.AxisY.Add(cadenceAxis);
            }
            if (showAnyStrideLength)
            {
                var strideLength = new Axis()
                {
                    Title = "StrideLength (m)",
                    Foreground = System.Windows.Media.Brushes.Teal,
                    LabelFormatter = value => value.ToString("0.0"),
                    Position = AxisPosition.RightTop
                };
                strideLengthAxisIndex = TimeChart.AxisY.Count;
                TimeChart.AxisY.Add(strideLength);
            }

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
                    Stroke = TimeChart.AxisY[speedAxisIndex].Foreground,
                    ScalesYAt = speedAxisIndex
                };
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowPace)
            {
                var speeds = ViewModel.GetTimeLine<double>(nameof(TrackPoint.SpeedKmH), numPoints);
                var series = new LineSeries
                {
                    Title = "Pace",
                    Values = new ChartValues<double>(speeds),
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    LineSmoothness = 1,
                    LabelPoint = chartPoint => $"{new Pace(chartPoint.Y).ToString()}min/km",
                    Stroke = TimeChart.AxisY[paceAxisIndex].Foreground,
                    ScalesYAt = paceAxisIndex
                };
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowHeartRate)
            {
                var heartRates = ViewModel.GetTimeLine<int>(nameof(TrackPoint.HeartRateBpm), numPoints).Select(hr => ((double)hr)).ToList();
                var series = new LineSeries
                {
                    Title = "HeartRate",
                    Values = new ChartValues<double>(heartRates),
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    LineSmoothness = 0,
                    LabelPoint = chartPoint => $"{chartPoint.Y.ToString("0")}bpm",
                    Stroke = TimeChart.AxisY[heartRateAxisIndex].Foreground,
                    ScalesYAt = heartRateAxisIndex
                };
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowTargetSpeed)
            {
                var targetSpeeds = ViewModel.GetTimeLine<double>(nameof(LapViewModel.TargetSpeed), numPoints);
                var series = new StepLineSeries
                {
                    Title = "Target Speed",
                    Fill = Brushes.Transparent,
                    Values = new ChartValues<double>(targetSpeeds),
                    PointGeometry = null,
                    AlternativeStroke = Brushes.Transparent,
                    Stroke = TimeChart.AxisY[speedAxisIndex].Foreground.Clone(),
                    ScalesYAt = speedAxisIndex
                };
                series.Stroke.Opacity = 0.7;
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowTargetPace)
            {
                var targetSpeeds = ViewModel.GetTimeLine<double>(nameof(LapViewModel.TargetSpeed), numPoints);
                var series = new StepLineSeries
                {
                    Title = "Target Pace",
                    Fill = Brushes.Transparent,
                    Values = new ChartValues<double>(targetSpeeds),
                    PointGeometry = null,
                    AlternativeStroke = Brushes.Transparent,
                    LabelPoint = chartPoint => $"{new Pace(chartPoint.Y).ToString()}min/km",
                    Stroke = TimeChart.AxisY[paceAxisIndex].Foreground.Clone(),
                    ScalesYAt = paceAxisIndex
                };
                series.Stroke.Opacity = 0.7;
                seriesCollection.Add(series);
            }

            if (ViewModel.DoShowCadence)
            {
                if (maxCadence > 0)
                {
                    var cadences = ViewModel.GetTimeLine<double>(nameof(TrackPoint.RunCadence), numPoints);
                    cadences = cadences.Select(c => c * 2).ToList();
                    var series = new LineSeries
                    {
                        Title = "Cadence",
                        Values = new ChartValues<double>(cadences),
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        LineSmoothness = 0,
                        LabelPoint = chartPoint => $"{chartPoint.Y.ToString("0")} steps/min",
                        Stroke = TimeChart.AxisY[cadenceAxisIndex].Foreground,
                        ScalesYAt = cadenceAxisIndex
                    };
                    seriesCollection.Add(series);
                }
            }

            if (ViewModel.DoShowStrideLength)
            {
                if (maxStrideLength > 0)
                {
                    var strideLengths = ViewModel.GetTimeLine<double>(nameof(TrackPoint.StrideLengthM), numPoints);
                    var series = new LineSeries
                    {
                        Title = "StrideLength",
                        Values = new ChartValues<double>(strideLengths),
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        LineSmoothness = 0,
                        LabelPoint = chartPoint => $"{chartPoint.Y.ToString("0.0")} m",
                        Stroke = TimeChart.AxisY[strideLengthAxisIndex].Foreground,
                        ScalesYAt = strideLengthAxisIndex
                    };
                    seriesCollection.Add(series);
                }
            }

            if (ViewModel.DoShowElevation)
            {
                if (maxElevation > 0)
                {
                    var elevations = ViewModel.GetTimeLine<double>(nameof(TrackPoint.AltitudeMeters), numPoints);
                    var series = new LineSeries
                    {
                        Title = "Elevation",
                        Values = new ChartValues<double>(elevations),
                        PointGeometry = null,
                        Fill = Brushes.Transparent,
                        LineSmoothness = 0,
                        LabelPoint = chartPoint => $"{chartPoint.Y.ToString("0.0")} m",
                        Stroke = TimeChart.AxisY[elevationAxisIndex].Foreground,
                        ScalesYAt = elevationAxisIndex
                    };
                    seriesCollection.Add(series);
                }
            }

            if (ViewModel.DoShowElevationChange && ViewModel.Laps.SelectMany(l => l.TrackPoints).Any())
            {
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
                        var series = new LineSeries
                        {
                            Title = "Elevation Change",
                            Values = new ChartValues<double>(elevationChanges),
                            PointGeometry = null,
                            Fill = Brushes.Transparent,
                            LineSmoothness = 0,
                            LabelPoint = chartPoint => $"{chartPoint.Y} m",
                            Stroke = TimeChart.AxisY[elevationAxisIndex].Foreground.Clone(),
                            ScalesYAt = elevationAxisIndex
                        };
                        series.Stroke.Opacity = 0.7;
                        seriesCollection.Add(series);
                    }
                }
            }

            TimeChart.Series.AddRange(seriesCollection);

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
