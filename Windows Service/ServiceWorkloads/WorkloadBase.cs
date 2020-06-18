using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace ServiceWorkloads
{
    public class WorkloadBase : IWorkload
    {
        #region  Private variables
        private bool isRunning = false;
        private ILog logger = null;
        private Guid id = new Guid();
        #endregion

        #region Public Properties
        public virtual int SleepInterval { get; set; }
        public virtual bool IsRunning { get => isRunning; set => isRunning = value; }

        public Guid Id => id;

        #endregion

        #region Events
        public event EventHandler WorkloadFinished;
        #endregion

        #region Protected Properties
        protected virtual ILog Logger
        {
            get
            {
                if (logger == null)
                {
                    logger = LogManager.GetLogger(GetType());
                }
                return logger;
            }
        }
        #endregion

        #region  Constructors
        public WorkloadBase()
        {
            // Default to 1 minute
            SleepInterval = 60;
            isRunning = false;
        }
        #endregion

        #region Methods

        protected virtual String GetLoggerName()
        {
            return GetType().ToString();
        }

        protected virtual void ValidateProperties()
        {

        }
        public virtual void Start()
        {
            ValidateProperties();
            isRunning = true;
            Logger.Debug($"Workload<{GetType().ToString()}> Started.");
        }

        public virtual void Stop()
        {
            isRunning = false;
            WorkloadFinished?.Invoke(this, new EventArgs());
            Logger.Debug($"Workload<{GetType().ToString()}> Stopped.");
        }

        protected String GetSetting (String key)
        {
            return Helpers.GetSetting(key);   
        }
        #endregion

        #region Event Handlers
        public virtual void StopEventHandler(object sender, EventArgs e)
        {
            Stop();
        }

        public virtual void StartEventHandler(object sender, EventArgs e)
        {
            Start();
        }
        #endregion

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
            {
                if (Id == ((IWorkload) obj).Id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }
    }
}
