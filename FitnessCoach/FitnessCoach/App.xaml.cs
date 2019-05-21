using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FitnessCoach.Config;
using FitnessCoach.Util;

namespace FitnessCoach
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("App");

        protected override void OnStartup(StartupEventArgs e)
        {
            GlobalConfig.Init();
            Application.Current.Activated += Current_Activated;
            Application.Current.Deactivated += Current_Deactivated;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error($"程序发生错误：{e.Exception.Message};错误地址：{e.Exception.StackTrace.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()}");
        }

        private void Current_Deactivated(object sender, EventArgs e)
        {
            SystemSleepManagement.RestoreSleep();
        }

        private void Current_Activated(object sender, EventArgs e)
        {
            SystemSleepManagement.PreventSleep();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}