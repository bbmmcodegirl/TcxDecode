﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TcxDecode
{
    [Serializable]
    public class Activity
    {
        public string Name { get; set; }

        public string Sport { get; set; }

        public string Creator { get; set; }

        public Lap[] Laps { get; set; } = new Lap[] { };

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

    [Serializable]
    public class TrackPoint
    {
        public DateTime Time { get; set; }

        public Position Position { get; set; }

        public double AltitudeMeters { get; set; }

        public double DistanceMeters { get; set; }

        public int HeartRateBpm { get; set; }

        public double Speed { get; set; }

        public double SpeedKmH { get => Speed * 3.6; }

        public double RunCadence { get; set; }

        public double StrideLengthM { get =>
                    RunCadence == 0 
                        ? 0 
                        : (SpeedKmH * 1000/60.0) // m per min == distance in m covered per min
                           / (RunCadence*2) // distance in m covered per stride
                ; }

        public TimeSpan Interval { get; set; }
        public double DistanceCoveredMeters { get; set; }

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
            if (heartRateElement != null)
            {
                var heartRateValue = heartRateElement.Descendants().Where(d => d.Name.LocalName == "Value").Select(e => e.Value).FirstOrDefault();
                if (TcxParser.ParseInt(heartRateValue, out int heartRate))
                {
                    trackPoint.HeartRateBpm = heartRate;
                }
            }
            else
            {
                trackPoint.HeartRateBpm = 0;
            }

            return trackPoint;
        }

    }

    [Serializable]
    public class Position
    {
        public double LatitudeDegrees { get; set; }
        public double LongitudeDegrees { get; set; }

        [XmlIgnore]
        public GeoCoordinate Coordinate { get => new GeoCoordinate(LatitudeDegrees, LongitudeDegrees); }

        public double DistanceTo(Position other)
        {
            return this.Coordinate.GetDistanceTo(other.Coordinate);
        }

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

    [Serializable]
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

    [Serializable]
    public class Lap
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public float TotalTimeSeconds { get; set; }
        public float DistanceMeters { get; set; }
        public int Calories { get; set; }
        public string Intensity { get; set; }
        public string TriggerMethod { get; set; }
        public Pace TargetPace { get; set; }
        public Pace TargetMinPace { get; set; }
        public Pace TargetMaxPace { get; set; }
        public Track Track { get; set;  } = new Track();
        public float AverageSpeedValue { get; set; }
        public float MaximumSpeedValue { get; set; }
        public int AverageHeartRateBpmValue { get; set; }
        public int MaximumHeartRateBpmValue { get; set; }
        public int AverageRunCadenceValue { get; set; }

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

            var averageSpeedValue = element.Descendants().Where(d => d.Name.LocalName == "AvgSpeed").Select(e => e.Value).FirstOrDefault();
            var maximumSpeedValue = element.Descendants().Where(d => d.Name.LocalName == "MaximumSpeed").Select(e => e.Value).FirstOrDefault();
            var averageHeartRateBpmValue = element.Descendants().Where(d => d.Name.LocalName == "AverageHeartRateBpm").Select(e => e.Descendants().Where(d => d.Name.LocalName == "Value").Select(v => v.Value).FirstOrDefault()).FirstOrDefault();
            var maximumHeartRateBpmValue = element.Descendants().Where(d => d.Name.LocalName == "MaximumHeartRateBpm").Select(e => e.Descendants().Where(d => d.Name.LocalName == "Value").Select(v => v.Value).FirstOrDefault()).FirstOrDefault();
            var averageRunCadenceValue = element.Descendants().Where(d => d.Name.LocalName == "AvgRunCadence").Select(e => e.Value).FirstOrDefault();

            if (TcxParser.ParseFloat(distanceValue, out float distance))
            {
                lap.DistanceMeters = distance;
            }

            if (TcxParser.ParseFloat(averageSpeedValue, out float averageSpeed))
            {
                lap.AverageSpeedValue = averageSpeed;
            }

            if (TcxParser.ParseFloat(totalSecondsValue, out float totalSeconds))
            {
                if (totalSeconds > 0)
                {
                    lap.TotalTimeSeconds = totalSeconds;
                }
                else if (lap.DistanceMeters > 0 && lap.AverageSpeedValue > 0)
                {
                    lap.TotalTimeSeconds = lap.DistanceMeters/lap.AverageSpeedValue;
                }
            }

            if (TcxParser.ParseFloat(maximumSpeedValue, out float maximumSpeed))
            {
                lap.MaximumSpeedValue = maximumSpeed;
            }

            if (TcxParser.ParseInt(averageHeartRateBpmValue, out int averageHeartRateBpm))
            {
                lap.AverageHeartRateBpmValue = averageHeartRateBpm;
            }

            if (TcxParser.ParseInt(maximumHeartRateBpmValue, out int maximumHeartRateBpm))
            {
                lap.MaximumHeartRateBpmValue = maximumHeartRateBpm;
            }

            if (TcxParser.ParseInt(caloriesValue, out int calories))
            {
                lap.Calories = calories;
            }

            if (TcxParser.ParseInt(averageRunCadenceValue, out int averageRunCadence))
            {
                lap.AverageRunCadenceValue = averageRunCadence;
            }

            lap.Intensity = intensityValue;
            lap.TriggerMethod = triggerMethodValue;

            var trackElement = element.Descendants().Where(d => d.Name.LocalName == "Track").FirstOrDefault();

            if (trackElement != null)
            {
                lap.Track = Track.Parse(trackElement);
            }
            else
            {
                lap.Track = new Track();
            }

            return lap;
        }

        public List<Lap> Split(double distanceMeters)
        {
            var list = new List<Lap>();
            int i = 0;
            do
            {
                var newTrackPoints = Track.TrackPoints.SkipWhile(t => t.DistanceMeters < distanceMeters * i).TakeWhile(t => t.DistanceMeters < distanceMeters * (i + 1)).ToList();
                if (!newTrackPoints.Any())
                {
                    break;
                }
                var subTrack = new Track()
                {
                    TrackPoints = newTrackPoints.ToArray()
                };
                // TODO make the laps exactly the required distance
                var partialLap = new Lap()
                {
                    Name = $"{Name}{i + 1}",
                    StartTime = newTrackPoints.OrderBy(t => t.Time).Select(t => t.Time).First(),
                    TotalTimeSeconds = (float)(newTrackPoints.OrderBy(t => t.Time).Select(t => t.Time).Last() - newTrackPoints.OrderBy(t => t.Time).Select(t => t.Time).First()).TotalSeconds,
                    DistanceMeters = (float)(newTrackPoints.Max(t => t.DistanceMeters) - newTrackPoints.Min(t => t.DistanceMeters)),
                    Intensity = this.Intensity,
                    TriggerMethod = i == 0 ? this.TriggerMethod : "Fixed distance",
                    Track = subTrack,
                    AverageSpeedValue = (float)newTrackPoints.Average(t => t.Speed),
                    MaximumSpeedValue = (float)newTrackPoints.Max(t => t.Speed),
                    AverageHeartRateBpmValue = (int)Math.Round(newTrackPoints.Average(t => (double)t.HeartRateBpm)),
                    MaximumHeartRateBpmValue = newTrackPoints.Max(t => t.HeartRateBpm),
                    AverageRunCadenceValue = (int)Math.Round(newTrackPoints.Average(t => (double)t.RunCadence)),
                };
                list.Add(partialLap);
                i++;
            } while (true);
            return list;
        }
    }
}
