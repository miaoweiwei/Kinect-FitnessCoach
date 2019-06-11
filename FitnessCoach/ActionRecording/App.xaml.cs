using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FitnessCoach.Config;
using FitnessCoach.Util;
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
            Application.Current.Activated += Current_Activated;
            Application.Current.Deactivated += Current_Deactivated;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);
        }
        private void Current_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogUtil.Error(this, e.Exception);
        }

        private void Current_Deactivated(object sender, EventArgs e)
        {
            SystemSleepManagement.RestoreSleep();
            Debug.WriteLine("恢复系统休眠策略");
        }

        private void Current_Activated(object sender, EventArgs e)
        {
            SystemSleepManagement.PreventSleep();
            Debug.WriteLine("阻止系统休眠");
        }
        private static void LogInit()
        {
            string configFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var configFile = new FileInfo(configFilePath);
            XmlConfigurator.Configure(configFile);
        }
    }
}
