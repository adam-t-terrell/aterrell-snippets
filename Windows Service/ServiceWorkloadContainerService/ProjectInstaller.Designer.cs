namespace ServiceWorkloads
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.WorkloadContainerProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.WorkloadContainerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // WorkloadContainerProcessInstaller
            // 
            this.WorkloadContainerProcessInstaller.Password = null;
            this.WorkloadContainerProcessInstaller.Username = null;
            // 
            // WorkloadContainerServiceInstaller
            // 
            this.WorkloadContainerServiceInstaller.ServiceName = "WorkloadContainerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.WorkloadContainerProcessInstaller,
            this.WorkloadContainerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller WorkloadContainerProcessInstaller;
        private System.ServiceProcess.ServiceInstaller WorkloadContainerServiceInstaller;
    }
}