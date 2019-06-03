using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using log4net.Config;
using Microsoft.Kinect;

namespace FitnessCoach.Config
{
    public static class GlobalConfig
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GlobalConfig");

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
            //string configFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            string startDir = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = Path.Combine(startDir, "FitnessCoach.exe.config");
            var configFile = new FileInfo(configFilePath);
            XmlConfigurator.Configure(configFile);
        }

        public static void ConfigInit()
        {
            try
            {
                Log.Info("加载获取配置文件信息");

                string startDir = AppDomain.CurrentDomain.BaseDirectory;
                string modelDir = Path.Combine(startDir, "Model");

                var config = ConfigurationManager.OpenExeConfiguration(Path.Combine(startDir, "FitnessCoach.exe"));

                string modelPath = config.AppSettings.Settings["ModelDirPath"].Value;
                ModelDirPath = string.IsNullOrEmpty(modelPath) ? modelDir : modelPath;

                string actionModelDirPath = config.AppSettings.Settings["ActionModelDirPath"].Value;
                ActionModelDirPath = string.IsNullOrEmpty(actionModelDirPath) ? modelDir : actionModelDirPath;

                string templateModelPath = config.AppSettings.Settings["ActionModelDirPath"].Value;
                TemplateModelPath = string.IsNullOrEmpty(templateModelPath)
                    ? Path.Combine(modelDir, "Template.model")
                    : templateModelPath;
            }
            catch (Exception ex)
            {
                Log.Error("初始化配置文件异常:" + ex.Message);
            }
        }

        private static void GlobalVariableInit()
        {
            GlobalConfig.Sensor = KinectSensor.GetDefault();
        }
    }
}