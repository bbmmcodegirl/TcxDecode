using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TcxDecode
{
    [Serializable]
    public class Pace: IComparable, IComparable<Pace>
    {
        public Pace()
        {
            SpeedKmH = 0;
        }

        public Pace(double speedKmH)
        {
            this.SpeedKmH = speedKmH;
        }

        public static implicit operator Pace(double speedKmH)
        {
            if (speedKmH <= 0)
            {
                return new Pace(0);
            }
            return new Pace(speedKmH);
        }

        public Pace(TimeSpan timeSpan)
        {
            var totalSeconds = timeSpan.TotalSeconds;
            if (totalSeconds <= 0)
            {
                SpeedKmH = 0;
            }
            else
            {
                SpeedKmH = 3600.0 / totalSeconds;
            }
        }

        public static Pace Parse(string s)
        {
            var match = Regex.Match(s, @"^(\d\d?):(\d\d)$");
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                if (minutes == 0 && seconds == 0)
                {
                    return new Pace(0);
                }
                if (seconds < 60)
                {
                    var timeSpan = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
                    return new Pace(timeSpan);
                }
            }
            throw new Exception($"Invalid expression to denote a Pace: '{s}', expected mm:ss");
        }

        public double SpeedKmH { get; set; }

        public TimeSpan AsTimeSpan
        {
            get
            {
                if (SpeedKmH <= 0)
                {
                    return TimeSpan.FromSeconds(0);
                }
                return TimeSpan.FromSeconds(3600.0 / SpeedKmH);
            }
        }

        public override string ToString()
        {
            var paceAsTimeSpan = AsTimeSpan;

            return $"{paceAsTimeSpan.Minutes}:{paceAsTimeSpan.Seconds.ToString("00") }";
        }

        public int CompareTo(object obj)
        {
            if (obj is Pace pace)
            {
                return CompareTo(pace);
            }
            return 0;
        }

        public int CompareTo(Pace other)
        {
            return -this.SpeedKmH.CompareTo(other.SpeedKmH);
        }
    }
}
