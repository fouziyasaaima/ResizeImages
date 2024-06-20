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

    static void ProcessFolder(string folderPath, AppSettings settings)
    {
        string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
        // Create backup and resized folders in the root folder (abc)
        string rootFolderPath = Path.GetDirectoryName(folderPath);
        string backupImagesFolder = Path.Combine(rootFolderPath, "backupimages");
        string resizedImagesFolder = Path.Combine(rootFolderPath, settings.SubfolderName);

        if (!Directory.Exists(backupImagesFolder))
        {
            Directory.CreateDirectory(backupImagesFolder);
            Log($"Created subfolder for backup images: {backupImagesFolder}", settings.LogFilePath);
        }

        if (!Directory.Exists(resizedImagesFolder))
        {
            Directory.CreateDirectory(resizedImagesFolder);
            Log($"Created subfolder for resized images: {resizedImagesFolder}", settings.LogFilePath);
        }

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

                        // Calculate new dimensions while maintaining aspect ratio
                        int newWidth = originalWidth;
                        int newHeight = originalHeight;

                        if (originalWidth > settings.MaxWidth || originalHeight > settings.MaxHeight)
                        {
                            double aspectRatio = (double)originalWidth / originalHeight;

                            if (originalWidth > settings.MaxWidth)
                            {
                                newWidth = settings.MaxWidth;
                                newHeight = (int)(settings.MaxWidth / aspectRatio);
                            }

                            if (newHeight > settings.MaxHeight)
                            {
                                newHeight = settings.MaxHeight;
                                newWidth = (int)(settings.MaxHeight * aspectRatio);
                            }

                            resizeNeeded = true;
                        }

                        if (resizeNeeded)
                        {
                            /*string backupImagesFolder = Path.Combine(folderPath, "backupimages");
                            string resizedImagesFolder = Path.Combine(folderPath, settings.SubfolderName);

                            if (!Directory.Exists(resizedImagesFolder))
                            {
                                Directory.CreateDirectory(resizedImagesFolder);
                                Log($"Created subfolder for resized images: {resizedImagesFolder}", settings.LogFilePath);
                            }

                            if (!Directory.Exists(backupImagesFolder))
                            {
                                Directory.CreateDirectory(backupImagesFolder);
                                Log($"Created subfolder for backup images: {backupImagesFolder}", settings.LogFilePath);
                            }*/

                            // Backup original image
                            string backupFileName = Path.Combine(backupImagesFolder, Path.GetFileName(filePath));
                            File.Copy(filePath, backupFileName, true); // Overwrite if backup exists

                            // Resize image
                            image.Mutate(x => x.Resize(newWidth, newHeight));

                            // Save resized image
                            string newFileName = Path.Combine(resizedImagesFolder, Path.GetFileName(filePath));
                            image.Save(newFileName);

                            // Replace original with resized image
                            File.Copy(newFileName, filePath, true);

                            Log($"Resized: {filePath} to {newWidth}x{newHeight} and backed up original as {backupFileName}", settings.LogFilePath);
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
                }
            }
        }

        // Process subfolders recursively
        string[] subfolders = Directory.GetDirectories(folderPath);
        foreach (string subfolder in subfolders)
        {
            ProcessFolder(subfolder, settings);
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

        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string configFilePath = Path.Combine(exeDirectory, "appsettings.json");

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

        ProcessFolder(folderPath, settings);

        Log($"Image resizing completed.", settings.LogFilePath);
    }
}
