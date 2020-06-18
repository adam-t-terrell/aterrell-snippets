using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            AcsServiceInit();
        }
        private void AcsServiceInit()
        {
            String path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            String executable = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine("Path = {0}", path);
            Console.WriteLine("Executable = {0}", executable);

            String configFilePath = String.Format("{0}.config", executable);

            using (AppConfig.Change(configFilePath))
            {
                Installers.Clear();

                
                WorkloadContainerProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
                WorkloadContainerServiceInstaller = new System.ServiceProcess.ServiceInstaller();

                // Default to LocalService account.  Admins can reset to domain account after install
                // if needed.
                WorkloadContainerProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
                WorkloadContainerProcessInstaller.Password = null;
                WorkloadContainerProcessInstaller.Username = null;

                Console.WriteLine("Parsing config file");
                foreach (String key in ConfigurationManager.AppSettings.AllKeys)
                {
                    Console.WriteLine("{0} = '{1}'", key, ConfigurationManager.AppSettings[key]);
                }

                WorkloadContainerServiceInstaller.Description = Helpers.GetSetting("ServiceDescription") ?? WorkloadContainerServiceInstaller.Description;
                WorkloadContainerServiceInstaller.DisplayName = Helpers.GetSetting("ServiceDisplayName") ?? WorkloadContainerServiceInstaller.DisplayName;
                WorkloadContainerServiceInstaller.ServiceName = Helpers.GetSetting("ServiceName") ?? WorkloadContainerServiceInstaller.ServiceName;

                Installers.AddRange(new System.Configuration.Install.Installer[] {
                    WorkloadContainerProcessInstaller,
                    WorkloadContainerServiceInstaller });

            }
        }

    }
}
