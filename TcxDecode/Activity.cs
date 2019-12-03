using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TcxDecode
{
    public class Activity
    {
        public string Sport { get; set; }

        public string Creator { get; set; }

        public Lap[] Laps { get; private set; } = new Lap[] { };

        public static Activity Parse(XElement element)
        {
            if (element.Name.LocalName != "Activity")
            {
                return null;
            }

            var activity = new Activity();

            var sportValue = element.Attributes().Where(d => d.Name.LocalName == "Sport").Select(a => a.Value).FirstOrDefault();
            activity.Sport = sportValue;

            var creatorElement = element.Descendants().Where(d => d.Name.LocalName == "Creator").FirstOrDefault();
            if (creatorElement != null)
            {
                var nameValue = creatorElement.Descendants().Where(d => d.Name.LocalName == "Name").Select(a => a.Value).FirstOrDefault();
                var idValue = creatorElement.Descendants().Where(d => d.Name.LocalName == "UnitId").Select(a => a.Value).FirstOrDefault();
                activity.Creator = $"{nameValue} {idValue}";
            }

            var lapElements = element.Descendants().Where(d => d.Name.LocalName == "Lap").ToList();
            var laps = lapElements.Select(e => Lap.Parse(e)).Where(t => t != null).ToList();
            activity.Laps = laps.ToArray();
            return activity;
        }
    }

    public class TrackPoint
    {
        public DateTime Time { get; set; }

        public Position Position { get; set; }

        public double AltitudeMeters { get; set; }

        public double DistanceMeters { get; set; }

        public int HeartRateBpm { get; set; }

        public double Speed { get; set; }

        public double RunCadence { get; set; }


        public static TrackPoint Parse(XElement element)
        {
            if (element.Name.LocalName != "Trackpoint")
            {
                return null;
            }

            var positionElement = element.Descendants().Where(d => d.Name.LocalName == "Position").FirstOrDefault();
            if (positionElement == null)
            {
                return null;
            }

            var position = Position.Parse(positionElement);
            if (position == null)
            {
                return null;
            }

            var timeValue = element.Descendants().Where(d => d.Name.LocalName == "Time").Select(e => e.Value).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(timeValue) || !DateTime.TryParse(timeValue, out DateTime time))
            {
                return null;
            }

            var trackPoint = new TrackPoint()
            {
                Position = position,
                Time = time
            };

            var altitudeValue = element.Descendants().Where(d => d.Name.LocalName == "AltitudeMeters").Select(e => e.Value).FirstOrDefault();
            var distanceValue = element.Descendants().Where(d => d.Name.LocalName == "DistanceMeters").Select(e => e.Value).FirstOrDefault();
            var speedValue = element.Descendants().Where(d => d.Name.LocalName == "Speed").Select(e => e.Value).FirstOrDefault();
            var cadenceValue = element.Descendants().Where(d => d.Name.LocalName == "RunCadence").Select(e => e.Value).FirstOrDefault();

            if (TcxParser.ParseDouble(altitudeValue, out double altitude))
            {
                trackPoint.AltitudeMeters = altitude;
            }

            if (TcxParser.ParseDouble(distanceValue, out double distance))
            {
                trackPoint.DistanceMeters = distance;
            }

            if (TcxParser.ParseDouble(speedValue, out double speed))
            {
                trackPoint.Speed = speed;
            }

            if (TcxParser.ParseInt(cadenceValue, out int cadence))
            {
                trackPoint.RunCadence = cadence;
            }

            var heartRateElement = element.Descendants().Where(d => d.Name.LocalName == "HeartRateBpm").FirstOrDefault();
            var heartRateValue = heartRateElement.Descendants().Where(d => d.Name.LocalName == "Value").Select(e => e.Value).FirstOrDefault();
            if (TcxParser.ParseInt(heartRateValue, out int heartRate))
            {
                trackPoint.HeartRateBpm = heartRate;
            }

            return trackPoint;
        }

    }

    public class Position
    {
        public double LatitudeDegrees { get; set; }
        public double LongitudeDegrees { get; set; }

        public static Position Parse(XElement element)
        {
            if (element.Name.LocalName != "Position")
            {
                return null;
            }

            var latitudeValue = element.Descendants().Where(d => d.Name.LocalName == "LatitudeDegrees").Select(e => e.Value).FirstOrDefault();
            var longitudeValue = element.Descendants().Where(d => d.Name.LocalName == "LongitudeDegrees").Select(e => e.Value).FirstOrDefault();

            if (!TcxParser.ParseDouble(latitudeValue, out double latitude))
            {
                return null;
            }

            if (!TcxParser.ParseDouble(longitudeValue, out double longitude))
            {
                return null;
            }

            return new Position()
            {
                LatitudeDegrees = latitude,
                LongitudeDegrees = longitude
            };
        }
    }

    public class Track
    {
        public TrackPoint[] TrackPoints { get; set; } = new TrackPoint[] { };

        public static Track Parse(XElement element)
        {
            if (element.Name.LocalName != "Track")
            {
                return null;
            }

            var trackPointElements = element.Descendants().Where(d => d.Name.LocalName == "Trackpoint").ToList();
            var trackPoints = trackPointElements.Select(e => TrackPoint.Parse(e)).Where(t => t != null).ToList();
            var track = new Track()
            {
                TrackPoints = trackPoints.ToArray()
            };
            return track;
        }
    }

    public class Lap
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public float TotalTimeSeconds { get; set; }
        public float DistanceMeters { get; set; }
        public int Calories { get; set; }
        public string Intensity { get; set; }
        public string TriggerMethod { get; set; }
        public Track Track { get; private set;  } = new Track();

        public static Lap Parse(XElement element)
        {
            if (element.Name.LocalName != "Lap")
            {
                return null;
            }

            var startTimeValue = element.Attributes().Where(d => d.Name.LocalName == "StartTime").Select(a => a.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(startTimeValue) || !DateTime.TryParse(startTimeValue, out DateTime startTime))
            {
                return null;
            }
            var lap = new Lap()
            {
                StartTime = startTime
            };

            var totalSecondsValue = element.Descendants().Where(d => d.Name.LocalName == "TotalTimeSeconds").Select(e => e.Value).FirstOrDefault();
            var distanceValue = element.Descendants().Where(d => d.Name.LocalName == "DistanceMeters").Select(e => e.Value).FirstOrDefault();
            var caloriesValue = element.Descendants().Where(d => d.Name.LocalName == "Calories").Select(e => e.Value).FirstOrDefault();
            var intensityValue = element.Descendants().Where(d => d.Name.LocalName == "Intensity").Select(e => e.Value).FirstOrDefault();
            var triggerMethodValue = element.Descendants().Where(d => d.Name.LocalName == "TriggerMethod").Select(e => e.Value).FirstOrDefault();

            if (TcxParser.ParseFloat(totalSecondsValue, out float totalSeconds))
            {
                lap.TotalTimeSeconds = totalSeconds;
            }

            if (TcxParser.ParseFloat(distanceValue, out float distance))
            {
                lap.DistanceMeters = distance;
            }

            if (TcxParser.ParseInt(caloriesValue, out int calories))
            {
                lap.Calories = calories;
            }

            lap.Intensity = intensityValue;
            lap.TriggerMethod = triggerMethodValue;

            var trackElement = element.Descendants().Where(d => d.Name.LocalName == "Track").FirstOrDefault();

            lap.Track = Track.Parse(trackElement);
            
            return lap;
        }
    }
}
