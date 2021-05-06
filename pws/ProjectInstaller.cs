using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Pws
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            ServiceInstaller si = findServiceInstaller("PHPCGIService");
            if (si != null)
            {
                "PHPCGIServie 设置卸载过程".Log();
                si.AfterUninstall += serviceInstaller_AfterUninstall;
            }
        }

        private void serviceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            foreach (Installer installer in Installers)
            {
                if (installer is ServiceInstaller)
                {
                    ServiceInstaller si = installer as ServiceInstaller;
                    using (ServiceController controller = new ServiceController(si.ServiceName))
                    {
                        controller.Start();
                    }
                }
            }
        }

        private void serviceInstaller_AfterUninstall(object sender, InstallEventArgs e)
        {
            PhpArchive.UninstallAll();
        }

        private ServiceInstaller findServiceInstaller(string name)
        {
            foreach (Installer installer in Installers)
            {
                if (installer is ServiceInstaller)
                {
                    ServiceInstaller si = installer as ServiceInstaller;
                    if (si.ServiceName.Equals(name))
                    {
                        return si;
                    }
                }
            }
            return null;
        }
    }
}
