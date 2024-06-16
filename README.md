Image Resizing Application
This C# application allows you to resize images in a specified folder based on configuration settings provided in appsettings.json. It uses SixLabors.ImageSharp library for image processing.

Features
Image Resizing: Resize images based on configured dimensions.
Configuration: Define target dimensions, supported file types, subfolder name for resized images, overwrite settings, and log file path in appsettings.json.
Logging: Errors and operations are logged to a specified log file (log.txt by default).
Requirements
.NET SDK installed on your machine.
Image files in the specified folder that you want to resize.
Configuration
Before running the application, ensure that appsettings.json is properly configured:

{
  "TargetDimensions": {
    "Width": 800,
    "Height": 600
  },
  "SupportedFileTypes": [ ".jpg", ".jpeg", ".png" ],
  "SubfolderName": "resizedimages",
  "OverwriteExisting": false,
  "LogFilePath": "log.txt",
  "MaxWidth": 800,
  "MaxHeight": 600
}

TargetDimensions: Width and height in pixels to resize images.
SupportedFileTypes: Array of file extensions (case insensitive) that the application will process.
SubfolderName: Name of the subfolder where resized images will be saved.
OverwriteExisting: If true, overwrite existing resized images with the same name.
LogFilePath: Path to the log file where operations and errors will be logged.
MaxWidth and MaxHeight: Maximum dimensions for images to trigger resizing.
Usage
Clone the repository to your local machine:
