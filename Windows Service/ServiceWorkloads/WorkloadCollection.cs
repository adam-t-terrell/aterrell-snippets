using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public enum WorkloadStatus { Stopped = 1, Started = 2, Disposed = 3 };

    public class WorkloadCollection: IList<IWorkload>
    {

        protected class WorkloadContainer
        {
            private IWorkload workload;
            private WorkloadStatus status = WorkloadStatus.Stopped;

            public WorkloadStatus Status { get => status; set => status = value; }
            public IWorkload Workload { get => workload; set => workload = value; }
        }

        protected List<WorkloadContainer> items = new List<WorkloadContainer>();

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public IWorkload this[int index] { get => items[index].Workload; set => items[index].Workload = value; }

        public event EventHandler WorkloadFinished;
        public event EventHandler OnStop;
        public event EventHandler OnStart;

        public WorkloadCollection () :base()
        {

        }

        public void Add(IWorkload w)
        {
            items.Add(new WorkloadContainer { Workload = w });

            OnStop += w.StopEventHandler;
            OnStart += w.StartEventHandler;

            w.WorkloadFinished += WorkloadFinishedHandler;
        }

        private void WorkloadFinishedHandler(object sender, EventArgs e)
        {
            IWorkload w = (IWorkload)sender;
            int i = IndexOf(w);
            if (i > 0)
            {
                items[i].Status = WorkloadStatus.Stopped;
            }

            // if no workloads running, then we are done.
            bool bDone = !(from x in items
                          where x.Status == WorkloadStatus.Started
                          select x).Any();

            if (bDone)
            {
                WorkloadFinished?.Invoke(this, e);
            }
        }

        public void Start ()
        {
            OnStart?.Invoke(this, new EventArgs());
        }
        public void Stop ()
        {
            OnStop?.Invoke(this, new EventArgs());
        }

        public void StopEventHandler(object sender, EventArgs e)
        {
            Stop();
        }

        public int IndexOf(IWorkload item)
        {
            var c = items.Where(x => x.Workload == item).SingleOrDefault();
            return items.IndexOf(c);
            
        }

        public void Insert(int index, IWorkload item)
        {
            items.Insert(index, new WorkloadContainer { Workload = item });
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(IWorkload item)
        {
            return (from x in items
             where x.Workload == item
             select x).Any();
        }

        // Note:  this is not a copy, just putting an object reference 
        // in the array
        public void CopyTo(IWorkload[] array, int arrayIndex)
        {
            for (int i=0;i<items.Count;i++)
            {
                array[arrayIndex + i] = items[i].Workload;
            }
        }

        public bool Remove(IWorkload item)
        {
            int i = IndexOf(item);
            if (i > 0)
            {
                RemoveAt(i);
                return true;
            } else
            {
                return false;
            }
        }

        public IEnumerator<IWorkload> GetEnumerator()
        {
            return (from w in items
             select w.Workload).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (from w in items
                    select w.Workload).GetEnumerator();
        }
    }
}
