using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.Kinect;

namespace FitnessCoach.Config
{
    public static class GlobalConfig
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("RibbonMenu");
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "FitnessCoach.exe");
        /// <summary>
        /// 模型文件的文件夹路径
        /// </summary>
        public static string ModelDirPath;


        /// <summary>
        /// Kinect
        /// </summary>
        public static KinectSensor Sensor { get; private set; }

        //public static WindowsPreview.Kinect.KinectSensor MySensor;

        /// <summary>
        /// 全局变初始化
        /// </summary>
        public static void Init()
        {
            LogInit();
            ConfigInit();
            GlobalVariableInit();
        }

        private static void LogInit()
        {
            var configFile = new FileInfo(ConfigPath+".config");
            XmlConfigurator.Configure(configFile);
        }

        public static void ConfigInit()
        {
            try
            {
                var appDomain = AppDomain.CurrentDomain;
                Log.Info("加载获取配置文件信息");
                var config = ConfigurationManager.OpenExeConfiguration(ConfigPath);
                string modelPath = config.AppSettings.Settings["ModelDirPath"].Value;
                ModelDirPath = string.IsNullOrEmpty(modelPath) ? appDomain.BaseDirectory + "Model" : modelPath;
                //HistoryDataTimeout = int.Parse(config.AppSettings.Settings["historyDataTimeout"].Value);
                //IasTimeout = int.Parse(config.AppSettings.Settings["IasTimeout"].Value);
                //BasicPriceShowCount = int.Parse(config.AppSettings.Settings["MarketStreamShowCount"].Value);
                //TradeDealShowCount = int.Parse(config.AppSettings.Settings["TradeDealShowCount"].Value);
                //TfSpreadShowCount = int.Parse(config.AppSettings.Settings["TfSpreadShowCount"].Value);
                //DatumRateShowCount = int.Parse(config.AppSettings.Settings["DatumRateShowCount"].Value);
                //FixedRateBondFilter = config.AppSettings.Settings["FixedRateBondFilter"].Value;
                //SkinStyle = config.AppSettings.Settings["SkinStyle"].Value;
                //ExcelMaxShowCount = int.Parse(config.AppSettings.Settings["ExcelMaxShowCount"].Value);
                //CdhUserName = config.AppSettings.Settings["CdhUserName"].Value;
                //CdhPassword = config.AppSettings.Settings["CdhPassword"].Value;
                //if ((config.AppSettings.Settings.AllKeys.Contains("RunMode")))
                //{ RunMode = config.AppSettings.Settings["RunMode"].Value.ToUpper(); }
            }
            catch (Exception ex)
            {
                Log.Error("初始化配置文件异常:" + ex.Message);
            }
        }

        public static  void GlobalVariableInit()
        {
           GlobalConfig.Sensor=KinectSensor.GetDefault();
            //MySensor = WindowsPreview.Kinect.KinectSensor.GetDefault;

           
        }
    }
}