﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace livelywpf
{
    public static class FileOperations
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Opens the folder in file explorer; If file path is given, file is selected.
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFolder(string path)
        {
            try
            {
                //bug, disabled for now.
                if (Program.IsMSIX)
                {
                    _ = LaunchFolder(path);
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe"
                };
                if (File.Exists(path))
                {
                    startInfo.Arguments = "/select, \"" + path + "\"";
                }
                else if (Directory.Exists(path))
                {
                    startInfo.Arguments = "\"" + path + "\"";
                }
                else
                {
                    throw new FileNotFoundException();
                }
                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + "\n" + e.StackTrace);
            }
        }

        private static async Task LaunchFolder(string path)
        {
            var packagePath = path;
            var localFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path;
            var packageAppData = Path.Combine(localFolder, "Local", "Lively Wallpaper");
            if (path.Length > Program.AppDataDir.Count() + 1)
            {
                var tmp = Path.Combine(packageAppData, path.Remove(0, Program.AppDataDir.Count() + 1));
                if (File.Exists(tmp) || Directory.Exists(tmp))
                {
                    packagePath = tmp;
                }
            }

            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(packagePath));
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }

        /// <summary>
        /// Deletes file and folder contents of a directory (parent directory remains).
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>true if succes, false otherwise.</returns>
        public static bool EmptyDirectory(string directory)
        {
            var status = true;
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(directory);

                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                status = false;
            }
            return status;
        }

        /// <summary>
        /// Async folder delete operation after given delay.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="initialDelay"></param>
        /// <param name="retryDelay"></param>
        /// <returns>true if succes, false otherwise.</returns>
        public static async Task<bool> DeleteDirectoryAsync(string folderPath, int initialDelay = 1000, int retryDelay = 4000)
        {
            bool status = true;
            if (Directory.Exists(folderPath))
            {
                await Task.Delay(initialDelay);
                try
                {
                    await Task.Run(() => Directory.Delete(folderPath, true));
                }
                catch (Exception ex)
                {
                    Logger.Error("Folder Delete Failure: " + ex.Message + "\n" + "Retrying..");
                    await Task.Delay(retryDelay);
                    try
                    {
                        await Task.Run(() => Directory.Delete(folderPath, true));
                    }
                    catch (Exception ie)
                    {
                        Logger.Error("(Retry)Folder Delete Failure: " + ie.Message);
                        status = false;
                    }
                }
            }
            return status;
        }

        //ref: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// <summary>
        /// Synchronous directory copy operation.
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
