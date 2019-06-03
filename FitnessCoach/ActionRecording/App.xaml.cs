using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FitnessCoach.Config;
using log4net.Config;

namespace ActionRecording
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            
            GlobalConfig.Init();
            //LogInit();
            base.OnStartup(e);
        }
        private static void LogInit()
        {
            string configFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var configFile = new FileInfo(configFilePath);
            XmlConfigurator.Configure(configFile);
        }
    }
}
