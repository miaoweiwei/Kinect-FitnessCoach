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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GlobalConfig");

        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "FitnessCoach.exe");

        /// <summary>
        /// 模型文件的文件夹路径
        /// </summary>
        public static string ModelDirPath;

        /// <summary>
        /// 动作模型文件的文件夹路径
        /// </summary>
        public static string ActionModelDirPath;

        /// <summary>
        /// 模板文件路径
        /// </summary>
        public static string TemplateModelPath;

        /// <summary>
        /// KinectSensor
        /// </summary>
        public static KinectSensor Sensor { get; private set; }

        /// <summary>
        /// 系统初始化
        /// </summary>
        public static void Init()
        {
            LogInit();
            ConfigInit();
            GlobalVariableInit();
        }

        private static void LogInit()
        {
            var configFile = new FileInfo(ConfigPath + ".config");
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

                string actionModelDirPath = config.AppSettings.Settings["ActionModelDirPath"].Value;
                ActionModelDirPath = string.IsNullOrEmpty(actionModelDirPath)? appDomain.BaseDirectory + "Model" : actionModelDirPath;

                string templateModelPath = config.AppSettings.Settings["ActionModelDirPath"].Value;
                TemplateModelPath = string.IsNullOrEmpty(templateModelPath) ? appDomain.BaseDirectory + "Model/"+ "Template.model" : templateModelPath;
            }
            catch (Exception ex)
            {
                Log.Error("初始化配置文件异常:" + ex.Message);
            }
        }

        public static void GlobalVariableInit()
        {
            GlobalConfig.Sensor = KinectSensor.GetDefault();
        }
    }
}