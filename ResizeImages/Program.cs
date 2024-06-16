using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

class ResizeImages
{
    public class AppSettings
    {
        public TargetDimensions TargetDimensions { get; set; }
        public string[] SupportedFileTypes { get; set; }
        public string SubfolderName { get; set; }
        public bool OverwriteExisting { get; set; }
        public string LogFilePath { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
    }

    public class TargetDimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    static AppSettings LoadConfiguration(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException("Configuration file not found.", configFilePath);
        }

        string jsonContent = File.ReadAllText(configFilePath);
        return JsonSerializer.Deserialize<AppSettings>(jsonContent);
    }

    static void Log(string message, string logFilePath)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        Console.WriteLine(logMessage);
        if (!string.IsNullOrWhiteSpace(logFilePath))
        {
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
    }

    static void Main()
    {
        string configFilePath = "appsettings.json";

        // Load configuration
        AppSettings settings;
        try
        {
            settings = LoadConfiguration(configFilePath);
        }
        catch (Exception ex)
        {
            Log($"Error loading configuration: {ex.Message}", null);
            return;
        }

        // Get folder path from user
        Console.Write("Enter the full path to the folder containing the images: ");
        string folderPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            Log("Invalid folder path. Exiting.", settings.LogFilePath);
            return;
        }

        string resizedImagesFolder = Path.Combine(folderPath, settings.SubfolderName);

        // Ensure the resized images subfolder exists
        if (!Directory.Exists(resizedImagesFolder))
        {
            Directory.CreateDirectory(resizedImagesFolder);
            Log($"Created subfolder for resized images: {resizedImagesFolder}", settings.LogFilePath);
        }

        int newWidth = settings.TargetDimensions.Width;
        int newHeight = settings.TargetDimensions.Height;
        int maxWidth = settings.MaxWidth;
        int maxHeight = settings.MaxHeight;

        // Process each image in the folder
        string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
        int filesProcessed = 0;
        int errors = 0;

        foreach (string filePath in imageFiles)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            if (Array.Exists(settings.SupportedFileTypes, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    using (Image image = Image.Load(filePath))
                    {
                        bool resizeNeeded = false;

                        // Check if image exceeds specified dimensions
                        if (image.Width > maxWidth || image.Height > maxHeight)
                        {
                            resizeNeeded = true;
                        }

                        if (resizeNeeded)
                        {
                            // Resize the image
                            image.Mutate(x => x.Resize(newWidth, newHeight));

                            // Construct the new file path
                            string newFileName = Path.Combine(resizedImagesFolder, $"resized_{newWidth}x{newHeight}_{Path.GetFileName(filePath)}");

                            if (settings.OverwriteExisting || !File.Exists(newFileName))
                            {
                                // Save the resized image
                                image.Save(newFileName);
                                Log($"Resized: {filePath} to {newWidth}x{newHeight} and saved as {newFileName}", settings.LogFilePath);
                            }
                            else
                            {
                                Log($"Skipped saving {newFileName} as it already exists.", settings.LogFilePath);
                            }

                            filesProcessed++;
                        }
                        else
                        {
                            Log($"Skipped resizing {filePath} as it does not exceed the specified dimensions.", settings.LogFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error processing file {filePath}: {ex.Message}", settings.LogFilePath);
                    errors++;
                }
            }
        }

        // Summary of the operation
        Log($"Image resizing completed. {filesProcessed} files processed, {errors} errors encountered.", settings.LogFilePath);
    }
}
