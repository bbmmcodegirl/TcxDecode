using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using TcxDecode;
using Utilities;

namespace TcxChart
{
    public class TcxChartViewModel : ViewModel
    {
        string sourceDirectory = KnownFolders.GetPath(KnownFolder.Downloads);
        string targetDirectory = Path.Combine(KnownFolders.GetPath(KnownFolder.Documents), "fitnessData") ;

        public TcxChartViewModel()
        {
            if (!Directory.Exists(sourceDirectory))
            {
                Directory.CreateDirectory(sourceDirectory);
            }
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var sourceFiles = Directory.GetFiles(sourceDirectory, "*.tcx").ToList();
            var targetFiles = Directory.GetFiles(targetDirectory, "*.tcx").ToList();

            var files = sourceFiles.ToList();
            files.AddRange(targetFiles);

            var activities = new List<ActivityViewModel>();
            foreach (var file in files)
            {
                try
                {
                    var loadedActivities = new TcxParser().Parse(file);
                    if (loadedActivities != null)
                    {
                        activities.AddRange(loadedActivities.Select(a => new ActivityViewModel(file, a)));
                    }
                }
                catch { }
            }

            sourceFiles = Directory.GetFiles(sourceDirectory, "*.tcxml").ToList();
            targetFiles = Directory.GetFiles(targetDirectory, "*.tcxml").ToList();

            files = sourceFiles.ToList();
            files.AddRange(targetFiles);

            foreach (var file in files)
            {
                try
                {
                    var loadedActivities = loadActivities(file);
                    if (loadedActivities != null)
                    {
                        activities.AddRange(loadedActivities.Select(a => new ActivityViewModel(file, a)));
                    }
                }
                catch { }
            }

            var groupedActivities = activities.GroupBy(a => a.StartTime);
            var newestActivities = groupedActivities.Select(g => g.OrderByDescending(a => new FileInfo(a.FileName).LastWriteTime).First());

            foreach(var newestActivity in newestActivities.OrderByDescending(a => a.StartTime))
            {
                Activities.Add(newestActivity);
                newestActivity.PropertyChanged += someActivityChanged;
            }
        }

        private void someActivityChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsDirty))
            {
                Notify(nameof(IsDirty));
            }
        }

        public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();

        public void SaveActivitiesIfDirty()
        {
            var fileGroups = Activities.GroupBy(a => Path.GetFileName(a.FileName));
            foreach (var fileGroup in fileGroups)
            {
                if (!Activities.Any(a => a.IsDirty))
                {
                    continue;
                }
                var commonFile = fileGroup.Key;
                var commonFileBaseName = Path.GetFileNameWithoutExtension(commonFile);
                var targetFile = Path.Combine(targetDirectory, $"{commonFileBaseName}.tcxml");
                saveActivities(targetFile, fileGroup);
            }
        }

        private void saveActivities(string targetFile, IEnumerable<ActivityViewModel> activities)
        {
            try
            {
                using (var fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
                {
                    var s = new XmlSerializer(typeof(Activity[]));
                    var serializableActivities = activities.Select(a => (Activity)a).ToArray();
                    s.Serialize(fileStream, serializableActivities);
                }
                foreach(var activity in activities)
                {
                    activity.Saved();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error saving activites to {targetFile}: {ex.ToString()}");
            }
        }

        private List<ActivityViewModel> loadActivities(string sourceFile)
        {
            try
            {
                using (var fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                {
                    var s = new XmlSerializer(typeof(Activity[]));
                    var serializedActivities = (Activity[]) s.Deserialize(fileStream);
                    return serializedActivities.Select(a => new ActivityViewModel(sourceFile, a)).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activites from {sourceFile}: {ex.ToString()}");
            }
            return null;
        }

        private ActivityViewModel currentActivity;
        public ActivityViewModel CurrentActivity
        {
            get => currentActivity;
            set
            {
                currentActivity = value;
                Notify();
            }
        }

        public bool  IsDirty
        {
            get => Activities.Any(a => a.IsDirty);
        }

        internal void ActivityDeselected(ActivityViewModel a)
        {
            if (CurrentActivity == a)
            {
                CurrentActivity = null;
            }
        }

        internal void ActivitySelected(ActivityViewModel a)
        {
            if (CurrentActivity != a)
            {
                CurrentActivity = a;
            }
        }
    }
}
