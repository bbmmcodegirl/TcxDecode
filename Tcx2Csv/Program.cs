using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcxDecode;

namespace Tcx2Csv
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Error.WriteLine("Tcx To Csv Application");

            string fileName = null;
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: Tcx2Csv.exe <filename> ");
                return;
            }

            fileName = args[0];
            try
            {
                var parser = new TcxParser();
                var activities = parser.Parse(fileName);
                if (activities == null)
                {
                    Console.Error.WriteLine($"Invalid Tcx file '{fileName}' ");
                    return;
                }
                if (!activities.Any())
                {
                    Console.Error.WriteLine($"No activities found in Tcx file '{fileName}' ");
                    return;
                }
                int iA = 0;
                foreach (var activity in activities)
                {
                    iA++;
                    string outFilePath = findOutfileName(fileName, activity);

                    var allTrackPoints = activity.Laps.SelectMany((lap, i) => lap.Track.TrackPoints.Select(t => new { Lap = lap, LapIndex = i, TrackPoint = t })).ToList();
                    allTrackPoints = allTrackPoints.OrderBy(t => t.TrackPoint.Time).ToList();

                    if (!allTrackPoints.Any())
                    {
                        Console.Error.WriteLine($"No track data found in activity {iA} Tcx file '{fileName}' ");
                        continue;
                    }

                    var headers = $"LapIndex\t{nameof(Lap.Name)}\t{nameof(TrackPoint.Time)}\t{nameof(TrackPoint.DistanceMeters)}\t{nameof(TrackPoint.Speed)}\t{nameof(TrackPoint.AltitudeMeters)}\t{nameof(TrackPoint.HeartRateBpm)}";

                    var lines = new string[] { headers }.ToList();
                    lines.AddRange(
                        allTrackPoints.Select(t => $"{t.LapIndex}\t{t.Lap.Name}\t{t.TrackPoint.Time}\t{t.TrackPoint.DistanceMeters}\t{t.TrackPoint.Speed}\t{t.TrackPoint.AltitudeMeters}\t{t.TrackPoint.HeartRateBpm}")
                    );
                    File.WriteAllLines(outFilePath, lines);
                    Console.Error.WriteLine($"{activity.Sport} Activity from {activity.Laps.Min(l => l.StartTime)} with {activity.Laps.Count()} and {lines.Count - 1} trackPoints (max distance {allTrackPoints.Max(t => t.TrackPoint.DistanceMeters)}m) written to '{outFilePath}' ");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception parsing '{fileName}': {ex.ToString()}");
            }
        }

        private static string findOutfileName(string fileName, Activity activity)
        {
            var outFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.csv";
            var outFilePath = Path.Combine(Path.GetDirectoryName(fileName), outFileName);
            if (File.Exists(outFilePath))
            {
                outFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{activity.Sport}.csv";
                outFilePath = Path.Combine(Path.GetDirectoryName(fileName), outFileName);
                if (File.Exists(outFilePath))
                {
                    int i = 0;
                    do
                    {
                        i++;
                        outFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{activity.Sport}_{i}.csv";
                        outFilePath = Path.Combine(Path.GetDirectoryName(fileName), outFileName);
                    }
                    while (File.Exists(outFilePath));
                }
            }
            return outFilePath;
        }
    }
}
