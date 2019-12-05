# TcxDecode
To decode and process Tcx fitness tracking files.

Fitness data from GPS tracking devices can be stored in the .tcx format. E.g. Garmin Connect allows the export of activities as such a file format.

The xsd for the tcx (a variant of xml) files is pretty sparse, and more data can be derived from the data therein to make creating charts easier, so the classes in here are superclasses of the ones defined by the xsd. 

The decoder library TcxDecode.dll decodes and analyses the data.

The console program Tcx2Csv outputs the laps inthe file to a CSV.

The UI program TcxChart.exe 
* parses all *.tcx files in the user's Downloads directory
* in addition, reads in the old data files it saved with the extra data the user might have defined
* parses the activities out of the files and displays them in a list, ordered by time downwards
* When an activity is selected, displays its lap data in a grid and a line chart of the heartrate/speed/target speed .
  -- the target speed is extra data added onto the original data. The target speed for a lap has to be edited by a user and can be saved. 
     When saving files, the program writes them to a folder "fitnessData" under the user's Documents directory.
  -- Such extra data (e.g. target speed) create the advantage this viewer has over the website viewing services the fitness tracking providers, e.g. Garmin, currently offer.

TODOs:
* Chart Y axis for both speed and heartrate(different units)
* Live update of charts (proper data binding)
* Live picking up of new files downloaded (FileSystemWatcher on the Downloads directory)
* Grouping of activities, avoiding to overload the UI
* Creating of different charts with different options

5/12/2019