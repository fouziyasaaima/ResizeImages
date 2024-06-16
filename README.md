# ResizeImages
# Image Resizing Application
ResizeImages is a C# console application that allows you to resize images in a specified folder based on configuration settings provided in `appsettings.json`. It utilizes SixLabors.ImageSharp library for image processing.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [CodeExamples](#codeexamples) 

## Installation
1. **Clone the repository:**

   ```bash
   git clone https://github.com/fouziyasaaima/ResizeImages.git
   cd ResizeImages

2. **Restore dependencies:**
   ```bash
   dotnet restore

3. **Build the project:**
   ```bash
   dotnet build -c Release

or
  **Publish platform specific:**
  ```bash
   dotnet publish -c Release -r win-x64    # Windows
   dotnet publish -c Release -r linux-x64  # Linux
   dotnet publish -c Release -r osx-x64    # macOS
  ```


## Configuration
Update appsettings.json with your desired configuration before running the application. The configuration file should look like this:
```
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
```

**TargetDimensions:** Width and height in pixels for resizing images.

**SupportedFileTypes:** Array of file extensions (case insensitive) that the application will process.

**SubfolderName:** Name of the subfolder where resized images will be saved.

**OverwriteExisting:** If true, overwrite existing resized images with the same name.

**LogFilePath:** Path to the log file where operations and errors will be logged.

**MaxWidth and MaxHeight:** Maximum dimensions for images to trigger resizing.

## Usage
1. **Run the application:**
```
dotnet run --project ResizeImages.csproj
```

2. **Follow the prompts:**

Enter the full path to the folder containing the images you want to resize.
The application will process each image according to the configured settings.

## CodeExamples
```
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;

public class ImageProcessor
{
    public void ResizeImage(string imagePath, int newWidth, int newHeight)
    {
        try
        {
            using (Image image = Image.Load(imagePath))
            {
                if (image.Width > newWidth || image.Height > newHeight)
                {
                    image.Mutate(x => x.Resize(newWidth, newHeight));
                    image.Save(imagePath); // Overwrite the original image
                }
                else
                {
                    Console.WriteLine($"Skipping {imagePath}. Image dimensions are within the specified limits.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resizing image {imagePath}: {ex.Message}");
        }
    }
}
```



