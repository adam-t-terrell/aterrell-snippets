using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServiceWorkloads
{
    public class FolderWatcherWorkloadBase : WorkloadBase
    {
        #region Private Member Variables
        private DateTime lastProcessTime = DateTime.Now;
        private DateTime lastWatchdogTime = DateTime.Now;
        private object ProcessTimeLock = new object();
        private object WatchdogTimeLock = new object();

        private string sourceFolder;
        private int fileAccessTimeout;
        private FileSystemWatcher watcher;
        private string filter = "*.*";
        private bool includeSubdirectories = false;
        private int fileAccessDelay = 5000;
        private Thread tWatchdog;
        private List<Thread> processThreads = new List<Thread>();
        private int watchdogInterval = 5 * 60 * 1000; // default watchdog interval to 5 minutes
        private int processIdleSleepInterval = 1000; // default to 1 second
        private int threadShutdownTimeout = 5000;
        private ThreadSafeFileQueue fileQueue = new ThreadSafeFileQueue();
        private int numberOfThreads = 1;

        private Dictionary<int, DateTime> procThreadHeartbeats = new Dictionary<int, DateTime>();

        #endregion

        #region Public Properties

        public string SourceFolder  { get => sourceFolder; set => sourceFolder = value; }

        public string Filter { get => filter; set => filter = value; }
        public bool IncludeSubdirectories { get => includeSubdirectories; set => includeSubdirectories = value; }
        public int FileAccessTimeout { get => fileAccessTimeout; set => fileAccessTimeout = value; }
        public int FileAccessDelay { get => fileAccessDelay; set => fileAccessDelay = value; }

        public int WatchdogInterval { get => watchdogInterval; set => watchdogInterval = value; }

        public int ThreadShutdownTimeout { get => threadShutdownTimeout; set => threadShutdownTimeout = value; }
        public int NumberOfThreads { get => numberOfThreads; set => numberOfThreads = value; }
        public int ProcessIdleSleepInterval { get => processIdleSleepInterval; set => processIdleSleepInterval = value; }
        public DateTime LastProcessTime
        {
            get
            {
                lock (ProcessTimeLock)
                {
                    return lastProcessTime;
                }
            }
        }

        public DateTime LastWatchdogTime
        {
            get
            {
                lock (WatchdogTimeLock)
                {
                    return lastWatchdogTime;
                }
            }
        }

        #endregion

        private void UpdateLastProcessTime()
        {
            lock(ProcessTimeLock)
            {
                lastProcessTime = DateTime.Now;
            }
        }

        private void updateLastWatchdogTime()
        {
            lock (WatchdogTimeLock)
            {
                lastWatchdogTime = DateTime.Now;
            }
        }

        public FolderWatcherWorkloadBase() : base()
        {
            // Get default configuration settings
            sourceFolder = "";
            FileAccessTimeout = int.Parse(GetSetting("fileAccessTimeout") ?? "10000");
            Filter = "*.*";
            IncludeSubdirectories = false;
        }

        protected override void ValidateProperties()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                base.ValidateProperties();
            }
            catch (ConfigurationErrorsException e)
            {
                sb.AppendLine(e.Message);
            }

            if (String.IsNullOrEmpty(SourceFolder)) sb.AppendLine("FolderWatcherWorkloadBase: 'SourceFolder' config value not set");

            if (sb.Length > 0)
            {
                throw new ConfigurationErrorsException(sb.ToString());
            }
        }
        private void WatchdogThreadProc()
        {
            if (WatchdogInterval == 0)
            {
                return;
            }
            int iCount = 0;

            // Do not start processing until first watchdog interval has passed.  
            // This will allow processing threads to startup and begin working.

            while (IsRunning)
            {
                Thread.Sleep(WatchdogInterval);

                TimeSpan span = LastProcessTime - DateTime.Now;
                int ms = (int)span.TotalMilliseconds;

                // If files in queue and last process time was greater than watchdog interval, process
                if ((ms > WatchdogInterval) && (fileQueue.Count > 0))
                {
                    Logger.Debug("Watchdog process wakeup.");
                    UpdateLastProcessTime();
                    try
                    {
                        iCount = ProcessFolder(sourceFolder);
                        Logger.Debug($"Watchdog process found {iCount} unprocessed files");
                        if (iCount > 0)
                        {
                            RestartFileWatcher();
                            CheckThreads();
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        Logger.Debug($"Folder {sourceFolder} not accessible.");
                    }
                }
                Thread.Yield();
            }
        }

        private void ProcessorThreadProc()
        {
            Logger.Debug($"Thread {Thread.CurrentThread.ManagedThreadId} starting, {fileQueue.Count} items in queue.");

            while (IsRunning)
            {
                FileInfo current = null;
                try
                {
                    while ((fileQueue.Count > 0) && (IsRunning))
                    {
                        //Logger.Debug("Calling FileQueue.Dequeue()");
                        current = fileQueue.Dequeue();
                        //Logger.Debug("ProcessFileInternal(current)");
                        ProcessFileInternal(current);
                        //Logger.Debug("UpdateLastProcessTime()");
                        UpdateLastProcessTime();  // Update last process time so Watchdog Thread knows the thread is active.
                        procThreadHeartbeats[Thread.CurrentThread.ManagedThreadId] = DateTime.Now;
                        //Logger.Debug("Thread.Yield()");
                        Thread.Yield();
                    }
                }
                catch (FileNotFoundException)
                {
                    Logger.Debug($"Folder {sourceFolder} not accessible.");
                }
                catch (Exception e)
                {
                    Logger.Error($"processThreadProc encoutnered an exception: {e.Message}", e);
                }
                finally
                {
                    Thread.Yield();
                    int iSleepSeconds = ProcessIdleSleepInterval / 1000;
                    for (int i = 0; i < iSleepSeconds; i++)
                    {
                        Thread.Sleep(1000);
                        if (!IsRunning)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void CheckThreads()
        {
            if (IsRunning)
            {
                foreach (Thread t in processThreads)
                {
                    if (!t.IsAlive)
                    {
                        Logger.Debug($"Thread {t.ManagedThreadId} is not alive, restarting thread.");
                        t.Start();
                    }
                    else if (t.ThreadState != ThreadState.Running)
                    {
                        Logger.Debug($"Thread {t.ManagedThreadId} is not running, restarting thread.");
                        t.Start();
                    }
                }
            }
        }
        public override void Start()
        {
            base.Start();

            try
            {
                // Kill watchdog thread if it exists
                if (tWatchdog != null)
                {
                    Logger.Debug($"Shutting down watchdog thread {tWatchdog.ManagedThreadId}..");
                    tWatchdog.Join(new TimeSpan(0, 0, 0, 0, threadShutdownTimeout));
                    tWatchdog = null;
                }

                // Kill processor threads if they exist
                foreach (Thread t in processThreads)
                {
                    Logger.Debug($"Shutting down processor thread {t.ManagedThreadId}..0");
                    t.Join(new TimeSpan(0, 0, 0, 0, threadShutdownTimeout));
                }
                processThreads.Clear();
                procThreadHeartbeats.Clear();

                // Process any files in input folder
                ProcessFolder(sourceFolder);

                // Start watchdog thread
                if (WatchdogInterval > 0)
                {
                    tWatchdog = new Thread(new ThreadStart(WatchdogThreadProc));
                    Logger.Debug($"Starting WatchDog thread {tWatchdog.ManagedThreadId}...");
                    tWatchdog.Start();
                } else
                {
                    tWatchdog = null;
                }

                for (int i=0;i<NumberOfThreads;i++)
                {
                    // Start processor thread
                    Thread t = new Thread(new ThreadStart(ProcessorThreadProc));
                    processThreads.Add(t);
                    procThreadHeartbeats.Add(t.ManagedThreadId, DateTime.Now);
                    Logger.Debug($"Starting processor thread {t.ManagedThreadId}.");

                    t.Start();

                    Thread.Sleep(100);  // Sleep 100 ms to stagger thread starts
                }

                // Setup file watcher to process subsequent changes
                WatchFolderAndProcess();

            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
        }

        private void RestartProcessThreads()
        {
            // Kill processor threads if they exist
            foreach (Thread t in processThreads)
            {
                Logger.Debug($"Shutting down processor thread {t.ManagedThreadId}..0");
                t.Join(new TimeSpan(0, 0, 0, 0, threadShutdownTimeout));
            }
            processThreads.Clear();

            procThreadHeartbeats.Clear();

            // Process any files in input folder
            ProcessFolder(sourceFolder);

            for (int i = 0; i < NumberOfThreads; i++)
            {
                // Start processor thread
                Thread t = new Thread(new ThreadStart(ProcessorThreadProc));
                processThreads.Add(t);
                procThreadHeartbeats.Add(t.ManagedThreadId, DateTime.Now);
                Logger.Debug($"Starting processor thread {t.ManagedThreadId}.");
                t.Start();

                Thread.Sleep(100);  // Sleep 100 ms to stagger thread starts
            }

            // Setup file watcher to process subsequent changes
            WatchFolderAndProcess();

        }
        private void WatchFolderAndProcess()
        {
            watcher = new FileSystemWatcher()
            {
                Path = sourceFolder,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = IncludeSubdirectories,
                Filter = Filter
            };
            watcher.Error += new ErrorEventHandler(WatcherError);
            watcher.Renamed += new RenamedEventHandler(ProcessFile);
            watcher.Changed += new FileSystemEventHandler(ProcessFile);
            watcher.Created += new FileSystemEventHandler(ProcessFile);
            watcher.EnableRaisingEvents = true;
        }


        private void RestartFileWatcher ()
        {
            if (IsRunning)
            {
                watcher.EnableRaisingEvents = false;
                Thread.Sleep(1000);
                watcher.EnableRaisingEvents = true;
            }
        }
        private void WatcherError(object source, ErrorEventArgs args)
        {
            Logger.Error(args.GetException().Message);
            Logger.Error($"FileSystemWatcher exited unexpectedly.");

            if (!IsRunning)
            {
                StopEventHandler(this, args);
            } else
            {
                while (IsRunning)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        watcher.Dispose();

                        Logger.Info($"Attempting Workload Restart... ");
                        Start();
                        Logger.Info($"Workload Restarted successfully... ");
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Info($"{e.Message}");
                        Logger.Info($"Failed to restart workload... ");
                    }
                }
            }
        }
        private int ProcessFolder(String folder, int fileCount = 0)
        {
            
            if (!Directory.Exists(folder))
            {
                throw new FileNotFoundException($"Folder {folder} not accessible.");
            }            
            // Process subfolders first
            //string[] folders = Directory.GetDirectories(folder);
            //foreach (String dir in folders)
            //{
            //    if (!IsRunning)
            //    {
            //        break;
            //    }
            //    fileCount += ProcessFolder(dir, fileCount);
            //    Thread.Yield();
            //}

            // Now process current directory
            string[] files = Directory.GetFiles(folder);

            if (files.Length > 0)
            {
                foreach (string file in files)
                {
                    if (!IsRunning)
                    {
                        break;
                    }
                    try
                    {
                        //ProcessFileInternal(new FileInfo(file));
                        Logger.Debug($"Queueing file '{file}' for processing.");
                        fileQueue.Enqueue(new FileInfo(file));
                        fileCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, ex);
                    }
                    Thread.Yield();
                }
            }
            return fileCount;
        }

        public override void Stop()
        {
            IsRunning = false;

            Logger.Debug($"Stopping Folder Watcher Workload.");
            if (watcher != null)
            {
                Logger.Debug($"Stopping FileSystemWatcher");
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            // Clear File Queue
            Logger.Debug($"Clearing queue of {fileQueue.Count} items...");
            fileQueue.Clear();
            Logger.Debug($"Queue cleared.");

            Logger.Debug($"Shutting down {processThreads.Count} processing threads...");
            // Kill processor threads if they exist
            foreach (Thread t in processThreads)
            {
                Logger.Debug($"Shutting down thread {t.ManagedThreadId}..");
                t.Join(new TimeSpan(0, 0, 0, 0, threadShutdownTimeout));
            }

            Logger.Debug("Processing threads are shut down, clearing thread list.");

            processThreads.Clear();

            Logger.Debug("Thread list clear, now shutting down watchdog thread.");


            if (tWatchdog != null)
            {
                tWatchdog.Join(new TimeSpan(0, 0, 0, 0, threadShutdownTimeout));
                try
                {
                    tWatchdog.Abort();
                } catch (Exception)
                {
                }
                tWatchdog = null;
            }

            Logger.Debug("Watchdog shutdown, now calling base to signal completion");

            // Call base method last so that it signals watchers that 
            // processing is complete
            base.Stop();
        }

        private void ProcessFile(object source, RenamedEventArgs e)
        {
            if (!File.Exists(e.FullPath))
            {
                return;
            }

            try
            {
                fileQueue.Enqueue(new FileInfo(e.FullPath));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            } finally
            {

            }

        }
        private void ProcessFile(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
            {
                return;
            }
            try
            {
                fileQueue.Enqueue(new FileInfo(e.FullPath));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
        }

        private void ProcessFileInternal (FileInfo fSource)
        {
            if (fSource != null && fSource.Exists)
            {
                // Wait for FileAccessDelay milliseconds to give the file a chance to settle
                Thread.Sleep(FileAccessDelay);
                try
                {
                    Helpers.WaitForFile(fSource, FileAccessTimeout);
                    Logger.DebugFormat("Calling ProcessFile() for '{0}'", fSource.FullName);
                    ProcessFile(fSource);
                }
                catch (FileNotFoundException)
                {

                }
            }
        }
        protected virtual void ProcessFile(FileInfo fSource)
        {
        }

    }
}