using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.ComponentModel;
using System.ServiceProcess;

namespace IAMReader
{
	[RunInstaller(true)]
	public partial class SocketServiceInstaller : System.Configuration.Install.Installer
	{
		private ServiceInstaller serviceInstaller;
		private ServiceProcessInstaller processInstaller;

		public SocketServiceInstaller()
		{

			processInstaller = new ServiceProcessInstaller();
			serviceInstaller = new ServiceInstaller();

			processInstaller.Account = ServiceAccount.LocalSystem;
			serviceInstaller.StartType = ServiceStartMode.Automatic;
			serviceInstaller.ServiceName = "IAMReader";

			//var serviceDescription = "Reader service for DLMS meters";

			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}
	}
}