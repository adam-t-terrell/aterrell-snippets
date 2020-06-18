using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public static class Helpers
    {
        //public const uint ERROR_SHARING_VIOLATION = 0x80070020;
        public const int ERROR_SHARING_VIOLATION = -2147024864;

        // Safely return configuration value or null instead of throwing an exception
        // if he key does not exist.  Use with assignments like
        //
        // var val = GetSetting("foo") ?? "defaultValue";
        //
        public static String GetSetting(String key)
        {
            if (ConfigurationManager.AppSettings.ContainsKey(key))
            {
                return ConfigurationManager.AppSettings.Get(key);
            }
            else
            {
                return null;
            }
        }
        public static void IOWithRetry(Action action, int timeoutMs = 1000)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    action();
                    return;
                }
                catch (IOException e)
                {
                    // access error
                    if (e.HResult != ERROR_SHARING_VIOLATION)
                        throw;
                }
            }
            throw new TimeoutException("Timeout occurred in IOWithRetry() method.");
        }
        private static bool IsFileReady(FileInfo f)
        {
            try
            {
                using (FileStream inputStream = f.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (inputStream.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        // TODO:  This is likely not a sharing violation and needs to be mapped 
                        // to another exception.
                        throw new IOException("File is empty", ERROR_SHARING_VIOLATION);
                        
                    }

                }

            }
            catch (IOException)
            {
                throw;
            }

        }
        public static bool WaitForFile (FileInfo f, int timeout)
        {
            IOWithRetry(() => IsFileReady(f), timeout);
            return true;
        }

        public static String getRelativeFolder(String baseFolder, String folder)
        {
            FileInfo fiBase = new FileInfo(baseFolder);
            FileInfo fiFolder = new FileInfo(folder);
            baseFolder = fiBase.FullName;
            folder = fiFolder.FullName;

            if (!folder.StartsWith(baseFolder))
            {
                throw new ArgumentException(String.Format("Folder '{0}' is not a subfolder of folder '{1}'",
                    folder, baseFolder));
            }
            String relativeFolder = folder.Substring(baseFolder.Length, folder.Length - baseFolder.Length);
            
            if (relativeFolder.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativeFolder = relativeFolder.Substring(1);
            }
            return relativeFolder;
        }

        public static string MoveFile(String sourceFile, String outputRootFolder, String outputFilename = "", String sourceRootFolder = "")
        {
            // TODO:  Check existance of sourceFile, sourceRootFolder

            String relativeFolder = "";
            FileInfo fiSource = new FileInfo(sourceFile);

            if (outputFilename == "")
            {
                outputFilename = new FileInfo(sourceFile).Name;
            }

            if (sourceRootFolder != "")
            {
                relativeFolder = getRelativeFolder(sourceRootFolder, fiSource.DirectoryName);
            }

            DirectoryInfo diOutputRootFolder = new DirectoryInfo(outputRootFolder);
            if (!diOutputRootFolder.Exists)
            {
                diOutputRootFolder.Create();
            }

            DirectoryInfo diOutputFolder = new DirectoryInfo(Path.Combine(diOutputRootFolder.FullName,relativeFolder));
            if (!diOutputFolder.Exists)
            {
                diOutputFolder.Create();
            }

            FileInfo fiOutputFile = new FileInfo(outputFilename);
            String outFullFileName = String.Format(@"{0}\{1}{2}",
                diOutputFolder.FullName, fiOutputFile.BaseFilename(), fiOutputFile.Extension);

            for (int i = 0; File.Exists(outFullFileName); i++)
            {
                outFullFileName = String.Format(@"{0}\{1}_{2}{3}",
                    diOutputFolder.FullName, fiOutputFile.BaseFilename(), i, fiOutputFile.Extension);
            }
            fiSource.MoveTo(outFullFileName);
            return outFullFileName;
        }

    }
}
