using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using log4net;
using log4net.Config;
using System.Configuration;
using System.Threading;
using log4net.Repository;

namespace ServiceWorkloads
{
    public class WorkloadContainerManager
    {
        public const int defaultServiceShutdownTimeout = 10000;

        public static int inboundSleepInterval;

        public event EventHandler ServiceStopping;

        private ILog logger = null;
        private WorkloadCollection items = null;

        public string ServiceName { get; set; }
        protected ILog Logger
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

        public WorkloadContainerManager(string serviceName = "WorkloadContainerManager")
        {
            ServiceName = serviceName;

            // Initialize logger
            var logConfigFile = Helpers.GetSetting("logConfig") ?? "Log4Net.config";
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, logConfigFile)));
        }

        public void Start(string[] args)
        {
            Logger.Debug("OnStart");

            items = new WorkloadCollection();
            ServiceStopping += items.StopEventHandler;
            Logger.Debug("Reading configuration data");
            WorkloadConfigurationSection config = null;
            try
            {
                config =
                    (WorkloadConfigurationSection)ConfigurationManager.GetSection(
                    "WorkloadConfiguration");

                if (config.Workloads.Count == 0)
                {
                    Logger.Error("No workloads configured.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            foreach (WorkloadConfiguration wcfg in config.Workloads)
            {
                Logger.Debug($"Creating workload {wcfg.WorkloadName}.");
                try
                {
                    Logger.Debug($"Creating instance of {wcfg.WorkloadType}");
                    IWorkload w = null;
                    if (wcfg.WorkloadAssembly == "")
                    {
                        Logger.Debug("Loading from main assembly");
                        w = (IWorkload)Assembly.GetExecutingAssembly().CreateInstance(wcfg.WorkloadType);
                    }
                    else
                    {
                        Logger.Debug($"Loading from assembly '{wcfg.WorkloadAssembly}");
                        Assembly a = Assembly.Load(wcfg.WorkloadAssembly);
                        w = (IWorkload)a.CreateInstance(wcfg.WorkloadType);
                    }

                    if (w == null)
                    {
                        Logger.Error($"Unable to create instance of {wcfg.WorkloadType}");
                        continue;
                    }
                    foreach (WorkloadProperty wProp in wcfg.WorkloadProperties)
                    {
                        Logger.Debug($"Setting property [{wProp.PropertyName}] to '{wProp.PropertyValue}'.");
                        PropertyInfo p = w.GetType().GetProperty(wProp.PropertyName);
                        if (p != null)
                        {
                            if (p.PropertyType == typeof(string))
                            {
                                p.SetValue(w, wProp.PropertyValue);
                            }
                            else if (p.PropertyType == typeof(int?))
                            {
                                p.SetValue(w, int.Parse(wProp.PropertyValue));
                            }
                            else if (p.PropertyType == typeof(int))
                            {
                                p.SetValue(w, int.Parse(wProp.PropertyValue));
                            }
                            else if (p.PropertyType == typeof(bool))
                            {
                                p.SetValue(w, bool.Parse(wProp.PropertyValue));
                            }
                            else
                            {
                                Logger.Error($"Unsupported configuration property type {p.PropertyType}");
                            }
                        }
                    }
                    items.Add(w);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                }
            }


            Logger.Info($"{ServiceName} started.");
            items.Start();

        }

        public void Stop()
        {
            ServiceStopping?.Invoke(this, new EventArgs());
            Thread.Sleep(int.Parse(Helpers.GetSetting("serviceShutdownTimeout") ?? defaultServiceShutdownTimeout.ToString()));

            Logger.Info($"{ServiceName} stopped.");
        }
    }
}
