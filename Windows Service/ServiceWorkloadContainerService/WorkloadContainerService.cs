using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using log4net;
using log4net.Config;


namespace ServiceWorkloads
{
    public partial class WorkloadContainerService : ServiceBase
    {

        WorkloadContainerManager workloadManager;
        public WorkloadContainerService()
        {
            InitializeComponent();

            workloadManager = new WorkloadContainerManager(ServiceName);
        }

        protected override void OnStart(string[] args)
        {
            workloadManager.Start(args);
        }

        protected override void OnStop()
        {
            workloadManager.Stop();
        }
    }
}
