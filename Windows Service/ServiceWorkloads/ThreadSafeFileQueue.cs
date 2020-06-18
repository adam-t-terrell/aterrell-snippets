using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public class ThreadSafeFileQueue
    {
        private Queue<string> fileQueue = new Queue<string>();

        
        public void Enqueue(FileInfo f)
        {
            Monitor.Enter(fileQueue);
            try
            {
                if (f != null && f.Exists && !fileQueue.Contains(f.FullName))
                {
                    fileQueue.Enqueue(f.FullName);
                }
            }
            finally
            {
                Monitor.Exit(fileQueue);
            }
        }

        public FileInfo Dequeue()
        {
            FileInfo fRet = null;
            try
            {
                Monitor.Enter(fileQueue);
                if (fileQueue.Count > 0)
                {
                    var filename = fileQueue.Dequeue();
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        FileInfo fi = new FileInfo(filename);
                        if (fi.Exists)
                        {
                            fRet = fi;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(fileQueue);
            }

            return fRet;

        }

        public FileInfo Peek()
        {
            FileInfo fRet = null;
            try
            {
                Monitor.Enter(fileQueue);
                var filename = fileQueue.Peek();
                if (!String.IsNullOrWhiteSpace(filename))
                {
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Exists)
                    {
                        fRet = fi;
                    }
                }

            }
            finally
            {
                Monitor.Exit(fileQueue);
            }
            return fRet;
        }

        public int Count => fileQueue.Count;

        public void Clear()
        {
            try
            {
                Monitor.Enter(fileQueue);
                while (Count > 0)
                {
                    fileQueue.Dequeue();
                }
            } finally
            {
                Monitor.Exit(fileQueue);
            }
        }
    }
}
