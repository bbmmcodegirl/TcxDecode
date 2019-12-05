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
        }

        public List<TrackPoint> TrackPoints
        {
            get => lap.Track.TrackPoints.ToList();

        }

        public int Index
        {
            get; private set;
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
                notifySpeedChanged();
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

        public DateTime EndTime
        {
            get => StartTime + TimeSpan.FromSeconds(TotalTimeSeconds);
        }

        public double DistanceKm
        {
            get => DistanceMeters / 1000;
        }

        public double AverageSpeedMetresS
        {
            get => TotalTimeSeconds > 0 ? DistanceMeters / TotalTimeSeconds : 0;
        }

        public double AverageSpeedKmH
        {
            get => AverageSpeedMetresS * 3.6;
        }

        public Pace AveragePace
        {
            get => AverageSpeedKmH ;
        }

        public double MaxSpeedMetresS
        {
            get => lap.Track.TrackPoints.Max(t => t.Speed);
        }

        public double MaxSpeedKmH
        {
            get => MaxSpeedMetresS * 3.6;
        }

        public int MaxHeartRateBpm
        {
            get => lap.Track.TrackPoints.Max(t => t.HeartRateBpm);
        }

        public double AverageHeartRateBpm
        {
            get => lap.Track.TrackPoints.Average(t => (double)t.HeartRateBpm);
        }

        public Pace BestPace
        {
            get => MaxSpeedKmH;
        }

        private void notifySpeedChanged()
        {
            Notify(nameof(AverageSpeedMetresS));
            Notify(nameof(AverageSpeedKmH));
            Notify(nameof(AveragePace));
        }
    }
}
