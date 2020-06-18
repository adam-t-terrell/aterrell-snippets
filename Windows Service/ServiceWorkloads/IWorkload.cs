using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public interface IWorkload
    {
        void Start();

        void Stop();

        Guid Id { get; }

        event EventHandler WorkloadFinished;

        void StopEventHandler(object sender, EventArgs e);
        void StartEventHandler(object sender, EventArgs e);
    }
}
