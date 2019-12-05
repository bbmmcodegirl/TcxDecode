﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcxDecode;
using Utilities;

namespace TcxChart
{
    public class ActivityViewModel: EditableViewModel
    {
        private Activity activity;

        public ActivityViewModel(string fileName, Activity activity)
        {
            this.activity = activity;
            FileName = fileName;
            Laps = activity.Laps.OrderBy(l => l.StartTime).Select((l, i) => new LapViewModel(l, i+1)).ToList();
            foreach (var lap in Laps)
            {
                lap.PropertyChanged += someLapPropertyChanged;
            }
            analyseLaps();
        }

        private void analyseLaps()
        {
            var lDist = Laps.Sum(l => l.DistanceMeters);
            var tDist = Laps.SelectMany(l => l.TrackPoints).Max(t => t.DistanceMeters);
            if (lDist != tDist)
            {
                Debugger.Log(1, "", $"Laps inconsistent: trackpoints cover {100.0 * tDist / lDist}% of lap distance ({tDist} vs {lDist})\r\n");

            }
            TrackPoint lastTrackPoint = null;
            var totalDistance = 0.0;
            foreach (var lap in Laps)
            {
                totalDistance += lap.DistanceMeters;
                foreach (var trackpoint in lap.TrackPoints)
                {
                    if (lastTrackPoint != null)
                    {
                        var interval = trackpoint.Time - lastTrackPoint.Time;
                        var intervalS = interval.TotalSeconds;
                        var distanceCovered = trackpoint.DistanceMeters - lastTrackPoint.DistanceMeters;
                        lastTrackPoint.Interval = interval;
                        trackpoint.DistanceCoveredMeters = distanceCovered;
                    }
                    lastTrackPoint = trackpoint;
                }
            }
            lastTrackPoint.Interval = EndTime - lastTrackPoint.Time;
        }

        public string FileName { get; }

        private void someLapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Notify(nameof(IsDirty));
        }

        public static implicit operator Activity(ActivityViewModel activity)
        {
            return activity.activity;
        }

        public List<LapViewModel> Laps { get; private set; }

        public string Name
        {
            get => activity.Name;
            set
            {
                if (activity.Name == value)
                {
                    return;
                }
                activity.Name = value;
                Notify();
            }
        }

        public string Sport
        {
            get => activity.Sport;
            set
            {
                if (activity.Sport == value)
                {
                    return;
                }
                activity.Sport = value;
                Notify();
            }
        }

        public string Creator
        {
            get => activity.Creator;
            set
            {
                if (activity.Creator == value)
                {
                    return;
                }
                activity.Creator = value;
                Notify();
            }
        }

        public new bool IsDirty
        {
            get => base.IsDirty || Laps.Any(l => l.IsDirty);
        }

        public DateTime StartTime
        {
            get => Laps.OrderBy(l => l.StartTime).Select(s => s.StartTime).FirstOrDefault();
        }

        public DateTime EndTime
        {
            get => Laps.OrderBy(l => l.EndTime).Select(s => s.EndTime).LastOrDefault();
        }

        public TimeSpan Duration
        {
            get => EndTime - StartTime;
        }

        public double Distance
        {
            get => Laps.Sum(l => l.DistanceKm);
        }

        public double MaxSpeed
        {
            get => Laps.Max(h => h.MaxSpeedKmH);
        }

        public double AverageSpeed
        {
            get => Laps.SelectMany(l => l.TrackPoints).Average(t => t.Speed);
        }

        public Pace BestPace
        {
            get => MaxSpeed;
        }

        public Pace AveragePace
        {
            get => AverageSpeed;
        }

        public int MaxHeartRate
        {
            get => Laps.Max(h => h.MaxHeartRateBpm);
        }

        public int AverageHeartRate
        {
            get => (int)Math.Round(Laps.SelectMany(l => l.TrackPoints).Average(t => (double)t.HeartRateBpm));
        }

        public List<PropertyType> GetTimeLine<PropertyType>(string propertyName, int numPoints, double startFraction = 0, double lengthFraction = 1)
        {
            var startTime = StartTime + TimeSpan.FromSeconds(Duration.TotalSeconds * startFraction);
            var length = TimeSpan.FromSeconds(Duration.TotalSeconds * lengthFraction);
            var endTime = startTime + length;
            var interval = TimeSpan.FromSeconds(length.TotalSeconds/numPoints);

            var propertyType = typeof(PropertyType);
            var trackpointProperty = typeof(TrackPoint).GetProperties()
                .FirstOrDefault(p => propertyType.IsAssignableFrom(p.PropertyType) && 
                p.Name == propertyName && 
                p.GetMethod != null && 
                !p.GetMethod.GetParameters().Any());
            var lapProperty = typeof(LapViewModel).GetProperties()
                .FirstOrDefault(p => propertyType.IsAssignableFrom(p.PropertyType) &&
                p.Name == propertyName &&
                p.GetMethod != null &&
                !p.GetMethod.GetParameters().Any());
            if (trackpointProperty == null && lapProperty == null)
            {
                return null;
            }
            var noParameters = new object[] { };
            var result = Enumerable.Range(0, numPoints)
                .Select(i =>
                {
                    var offset = TimeSpan.FromSeconds((int)(length.TotalSeconds * i / numPoints));
                    var timePoint = startTime + offset;
                    var containingLaps = Laps.Where(l => l.StartTime <= timePoint && l.EndTime >= timePoint).ToList();
                    foreach (var lap in containingLaps)
                    {
                        if (trackpointProperty != null)
                        {
                            var trackPoint = lap.TrackPoints.FirstOrDefault(t => t.Time <= timePoint && t.Time + t.Interval > timePoint);
                            if (trackPoint == null)
                            {
                                var firstPoint = lap.TrackPoints.FirstOrDefault();
                                if (firstPoint != null)
                                {
                                    if (lap.StartTime <= timePoint && firstPoint.Time > timePoint)
                                    {
                                        if (lap.Index > 0)
                                        {
                                            var previousLap = Laps.FirstOrDefault(l => l.Index == lap.Index - 1);
                                            if (previousLap != null)
                                            {
                                                var previousPoint = previousLap.TrackPoints.LastOrDefault();
                                                if (previousPoint != null)
                                                {
                                                    trackPoint = previousPoint;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (trackPoint != null)
                            {
                                var value = (PropertyType)trackpointProperty.GetMethod.Invoke(trackPoint, noParameters);
                                return value;
                            }
                        }
                        if (lapProperty != null)
                        {
                            var value = (PropertyType)lapProperty.GetMethod.Invoke(lap, noParameters);
                            return value;
                        }
                    }
                    return default(PropertyType);
                }).ToList();
            return result;
        }

        public List<DateTime> GetTimeSeries(int numPoints, double startFraction = 0, double lengthFraction = 1)
        {
            var startTime = StartTime + TimeSpan.FromSeconds(Duration.TotalSeconds * startFraction);
            var length = TimeSpan.FromSeconds(Duration.TotalSeconds * lengthFraction);
            var endTime = startTime + length;

            var result = Enumerable.Range(0, numPoints)
                .Select(i =>
                {
                    var offset = TimeSpan.FromSeconds(length.TotalSeconds * i / numPoints);
                    var timePoint = startTime + offset;
                    return timePoint;
                }).ToList();
            return result;
        }

        public override string ToString()
        {
            var startTime = StartTime;
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return $"{Name} on {startTime.ToString("d/M/yyyy")} at {startTime.ToString("H:mm")}";
            }
            return $"{Sport} on {startTime.ToString("d/M/yyyy")} at {startTime.ToString("H:mm")}";
        }
    }
}
