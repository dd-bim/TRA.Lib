pkg load io
% read coordinates
% Read data from a delimiter-separated file (e.g., tab-separated)
data = dlmread(fullfile('TestResults','CoordinatesOut.txt'), ' '); % Specify the delimiter as '\t' for tab

% Define the file path
filePath = 'C:\HTW\Trassierung\Ausgabe.ods';

% Read the ODS file (Assuming you want to read the first sheet)
[odsdata, odstext, raw] = odsread(filePath);



% Display the data
figure
plot(data(:,2),data(:,1));
hold on
plot(data(:,2),data(:,1),'.');
plot(odsdata(:,5),odsdata(:,7),'o');
text(odsdata(:,5),odsdata(:,7), strcat("ID",num2str((1:length(odsdata(:,5)))')," ",odstext(1:length(odsdata(:,5)),14)));
axis equal






