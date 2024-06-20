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

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: ResizeImages <FolderPath>");
            return;
        }

        string folderPath = args[0];

        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            Console.WriteLine("Invalid folder path. Exiting.");
            return;
        }

        // Get the path of the directory where the executable is located
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string configFilePath = Path.Combine(exeDirectory, "appsettings.json");

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

        string resizedImagesFolder = Path.Combine(folderPath, settings.SubfolderName);
        string backupImagesFolder = Path.Combine(folderPath, "backupimages");

        // Ensure the resized images subfolder and backup subfolder exist
        if (!Directory.Exists(resizedImagesFolder))
        {
            Directory.CreateDirectory(resizedImagesFolder);
            Log($"Created subfolder for resized images: {resizedImagesFolder}", settings.LogFilePath);
        }

        if (!Directory.Exists(backupImagesFolder))
        {
            Directory.CreateDirectory(backupImagesFolder);
            Log($"Created subfolder for backup images: {backupImagesFolder}", settings.LogFilePath);
        }

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
                        int originalWidth = image.Width;
                        int originalHeight = image.Height;
                        bool resizeNeeded = false;

                        // Calculate the new dimensions while maintaining the aspect ratio
                        int newWidth = originalWidth;
                        int newHeight = originalHeight;

                        if (originalWidth > maxWidth || originalHeight > maxHeight)
                        {
                            double aspectRatio = (double)originalWidth / originalHeight;

                            if (originalWidth > maxWidth)
                            {
                                newWidth = maxWidth;
                                newHeight = (int)(maxWidth / aspectRatio);
                            }

                            if (newHeight > maxHeight)
                            {
                                newHeight = maxHeight;
                                newWidth = (int)(maxHeight * aspectRatio);
                            }

                            resizeNeeded = true;
                        }

                        if (resizeNeeded)
                        {
                            // Backup the original image
                            string backupFileName = Path.Combine(backupImagesFolder, Path.GetFileName(filePath));
                            File.Copy(filePath, backupFileName, true); // Overwrite if backup exists

                            // Resize the image
                            image.Mutate(x => x.Resize(newWidth, newHeight));

                            // Save the resized image in the resized images subfolder
                            string newFileName = Path.Combine(resizedImagesFolder, Path.GetFileName(filePath));
                            image.Save(newFileName);

                            // Replace the original image with the resized one
                            File.Copy(newFileName, filePath, true); // Overwrite the original file with the resized image

                            Log($"Resized: {filePath} to {newWidth}x{newHeight} and backed up original as {backupFileName}", settings.LogFilePath);

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
