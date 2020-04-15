using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcxDecode;
using Utilities;

namespace TcxChart
{
    public class LapViewModel: EditableViewModel
    {
        private Lap lap;

        public LapViewModel(Lap lap, int index)
        {
            this.lap = lap;
            Index = index;
            TrackPoint lastTrackPoint = null;
            foreach(var t in TrackPoints)
            {
                if (lastTrackPoint != null)
                {
                    var altituteChange = t.AltitudeMeters - lastTrackPoint.AltitudeMeters;
                    if (altituteChange> 0)
                    {
                        Ascent += altituteChange;
                    }
                    else
                    {
                        Descent += altituteChange;
                    }
                }
                lastTrackPoint = t;
            }
        }

        public List<TrackPoint> TrackPoints
        {
            get => lap.Track.TrackPoints.ToList();

        }

        public int Index
        {
            get; set;
        }

        public string Name
        {
            get => lap.Name;
            set
            {
                if (lap.Name == value)
                {
                    return;
                }
                lap.Name = value;
                Notify();
            }
        }

        public double TargetSpeed
        {
            get => lap.TargetPace?.SpeedKmH ?? 0;
            set
            {
                lap.TargetPace = value;
                Notify();
                Notify(nameof(TargetPace));
            }
        }

        public Pace TargetPace
        {
            get => lap.TargetPace;
            set
            {
                lap.TargetPace = value;
                Notify();
                Notify(nameof(TargetSpeed));
            }
        }

        public DateTime StartTime
        {
            get => lap.StartTime;
            set
            {
                if (lap.StartTime == value)
                {
                    return;
                }
                lap.StartTime = value;
                Notify();
                Notify(nameof(EndTime));
            }
        }

        public DateTime EndTime
        {
            get => StartTime + TimeSpan.FromSeconds(TotalTimeSeconds);
        }

        public float TotalTimeSeconds
        {
            get => lap.TotalTimeSeconds;
            set
            {
                if (lap.TotalTimeSeconds == value)
                {
                    return;
                }
                lap.TotalTimeSeconds = value;
                Notify();
                Notify(nameof(EndTime));
                Notify(nameof(Duration));
                notifySpeedChanged();
            }
        }

        public TimeSpan Duration
        {
            get => TimeSpan.FromSeconds(TotalTimeSeconds);
            set
            {
                TotalTimeSeconds = (float)value.TotalSeconds;
            }
        }


        public float DistanceMeters
        {
            get => lap.DistanceMeters;
            set
            {
                if (lap.DistanceMeters == value)
                {
                    return;
                }
                lap.DistanceMeters = value;
                Notify();
                Notify(nameof(DistanceKm));
                notifySpeedChanged();
            }
        }

        public double DistanceKm
        {
            get => DistanceMeters / 1000;
        }

        public double AverageSpeedMetresS
        {
            get
            {
                if (TotalTimeSeconds > 0)
                {
                    return DistanceMeters / TotalTimeSeconds;
                }
                if (lap.AverageSpeedValue > 0)
                {
                    return lap.AverageSpeedValue;
                }
                return 0;
            }
        }

        public double AverageSpeedKmH
        {
            get => AverageSpeedMetresS * 3.6;
        }

        public double SpeedKmH
        {
            get => AverageSpeedKmH;
        }

        public Pace AveragePace
        {
            get => AverageSpeedKmH ;
        }

        public double AverageHeartRateBpm
        {
            get 
            {
                if (lap.Track.TrackPoints.Any())
                {
                    var totalSeconds = lap.Track.TrackPoints.Sum(t => t.Interval.TotalSeconds);
                    return totalSeconds <= 0 ? 0 : lap.Track.TrackPoints.Sum(t => (double)t.HeartRateBpm * t.Interval.TotalSeconds) / totalSeconds;
                }
                else
                {
                    return lap.AverageHeartRateBpmValue;
                }
            }
        }

        public int HeartRateBpm
        {
            get => (int) Math.Round(AverageHeartRateBpm);
        }

        public double AverageRunCadence
        {
            get
            {
                if (lap.Track.TrackPoints.Any())
                {
                    var totalSeconds = lap.Track.TrackPoints.Sum(t => t.Interval.TotalSeconds);
                    return totalSeconds <= 0 ? 0 : lap.Track.TrackPoints.Sum(t => (double)t.RunCadence * 2 // run cadence measures double steps, but garmin device usually shows single steps
                        * t.Interval.TotalSeconds) / totalSeconds;
                }
                else
                {
                    return lap.AverageRunCadenceValue * 2; // run cadence measures double steps, but garmin device usually shows single steps
                }
            }
        }

        public double RunCadence
        {
            get => AverageRunCadence;
        }

        public double MaxSpeedMetresS
        {
            get => lap.Track.TrackPoints.Any() ? lap.Track.TrackPoints.Max(t => t.Speed) : lap.MaximumSpeedValue;
        }

        public double MaxSpeedKmH
        {
            get => MaxSpeedMetresS * 3.6;
        }

        public double StrideLengthM { get =>
                    RunCadence == 0 
                        ? 0 
                        : (AverageSpeedKmH * 1000/60.0) // m per min == distance in m covered per min
                           / RunCadence // distance in m covered per stride
                ; }

        public Pace BestPace
        {
            get => MaxSpeedKmH;
        }

        public int MaxHeartRateBpm
        {
            get => lap.Track.TrackPoints.Any() ? lap.Track.TrackPoints.Max(t => t.HeartRateBpm) : lap.MaximumHeartRateBpmValue;
        }

        public double Ascent
        {
            get; 
        }

        public double Descent
        {
            get;
        }

        public int Calories
        {
            get => lap.Calories;
            set
            {
                if (lap.Calories == value)
                {
                    return;
                }
                lap.Calories = value;
                Notify();
            }
        }


        public string Intensity
        {
            get => lap.Intensity;
            set
            {
                if (lap.Intensity == value)
                {
                    return;
                }
                lap.Intensity = value;
                Notify();
            }
        }


        public string TriggerMethod
        {
            get => lap.TriggerMethod;
            set
            {
                if (lap.TriggerMethod == value)
                {
                    return;
                }
                lap.TriggerMethod = value;
                Notify();
            }
        }

        private void notifySpeedChanged()
        {
            Notify(nameof(AverageSpeedMetresS));
            Notify(nameof(AverageSpeedKmH));
            Notify(nameof(AveragePace));
        }

        internal List<LapViewModel> Split(double distanceMeters)
        {
            var newLaps = this.lap.Split(distanceMeters);
            return newLaps.Select((l, i) => new LapViewModel(l, this.Index + i)).ToList();
        }
    }
}
