# TRA.Lib

## Description
	This repository consists of three projects.
## 1. TRA.Lib:
	Library providing Functionality for loading .TRA and .GRA files. If the Library is compiled using the symbol "USE_SCOTTPLOT" it is also providing a detailed visualisation of the loaded data.
### Geometry-Types
	TRA-Files consists of a collection of elements. Each element has a underlying Geometry-Type. Currently following Geometry-Types are supported:
	-Straight (Gerade)
	-Circle (Kreis)
	-Clothoid (Klothoid)
### Usage
	This is done by calling:
	```sh
    var trasse = Trassierung.ImportTRA(fileName);
	var gradient = Trassierung.ImportGRA(fileName);
    ```
	To calculate 2DInterpolation points for a TRA-File call
	```sh
    trasse.Interpolate(delta)
    ```
	providing a delta value as distance between the calculated points.
	GRA files are prividing height information related to a station-value S. This station-value is mostly defined in 2D-space via a Station-TRA file (Mileage/"Stationierungsachse"). TRA left/right Files have to be projected to this Station-TRA to retrieve first a value S from the requested 2D Position, and in a second step to calculate a height from the GRA-File on this value S. These relations between the files can be set by:
	```sh
    trasseL.AssignTrasseS(trasseS);
	trasseL.AssignGRA(gradientL);
    ```
	If no trasseS is set the original station-values S are used from the TRA-File and no projection is applied. After the GRA-File is assigned 3D-Points can be calculated by 
	```sh
    trasse.Interpolate3D(null, 10.0);
    ```
	the fist parameters alows to overwrite/set the TrasseS, if null the previous set is used.
	If the library is comiled using the symbol "USE_SCOTTPLOT" the loaded data can be visualized by calling:
	```sh
    trasse.Plot();
    ```
	Calling this, also inlcludes data of a assigned GRA-File.
	Finally exporting the data to CSV is possible by:
	```sh
    trasse.SaveCSV(StreamWriter);
    ```
	if a interpolation was done before this also includes the Interpolationpoints.
## 2.TRA.Lib_TEST:
	This project contains Unit-Tests to verify the calculations and functions implemented in TRA.Lib. Results are compared to expected values, especally relevant for interpolations of complex Geometries like Clothoids. Tests are grouped in different categories:
	- CoordinateTransformation (Transformation in local Geometryspace)
	- ElementEstimation (Get the relevant Element closest to a Point)
	- GeometryInterpolation (Interpolation of the different Geometry-Types)
	-GeometryProjection (Get a value s on a Gemeotry for a 2D-Point)
	-OverallExecutionTest (Test the whole workflow, loading files, interpolate and Plot)
## 3.TRA.Tool
	This project provides a executable UI and a simple usage of TRA.Lib.
### Usage
	In the explorer to the left, open the folder containig the relevant TRA and GRA files. The UI allows to load a set of files, like they are commonly used, containing TRA -left(L), -right(R), -milage(S) and GRA -left(L), right(R). Simply drag a file to the corresponding textbox, if the naming convention of the other files follows 'xxx[L/R/S].TRA' and 'xxx[L/R].GRA', the other fileds are set automatically. Afterwards a Interpolation can be calculated using the 'Interpolate'-Button (this opens automatically the Plot-View if the included TRA.Lib is compiled with "USE_SCOTTPLOT") and the Result can be exported using the 'SaveCSV' button.

