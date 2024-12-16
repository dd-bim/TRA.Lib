pkg load io
% read coordinates
% Read data from a delimiter-separated file (e.g., tab-separated)
data = dlmread(fullfile('TestResults','CoordinatesOut.txt'), ' '); % Specify the delimiter as '\t' for tab

% Define the file path
filePath = 'C:\HTW\Trassierung\Ausgabe.ods';



% Display the data
figure
hold on
plot(data(:,2),data(:,1),'.');
axis equal

% ODS Import
[data_6240046R, odstext_6240046R, raw] = odsread(filePath,'6240046R');
plot(data_6240046R(:,5),data_6240046R(:,7),'-o');
text(data_6240046R(:,5),data_6240046R(:,7), strcat("ID",num2str((1:length(data_6240046R(:,5)))')," ",odstext_6240046R(1:length(data_6240046R(:,5)),14)));
[data_6240046L, odstext_6240046L, raw] = odsread(filePath,'6240046L');
plot(data_6240046L(:,5),data_6240046L(:,7),'-o');
text(data_6240046L(:,5),data_6240046L(:,7), strcat("ID",num2str((1:length(data_6240046L(:,5)))')," ",odstext_6240046L(1:length(data_6240046L(:,5)),14)));
[data_6240046S, odstext_6240046S, raw] = odsread(filePath,'6240046S');
plot(data_6240046S(:,5),data_6240046S(:,7),'-o');
text(data_6240046S(:,5),data_6240046S(:,7), strcat("ID",num2str((1:length(data_6240046S(:,5)))')," ",odstext_6240046S(1:length(data_6240046S(:,5)),14)));







