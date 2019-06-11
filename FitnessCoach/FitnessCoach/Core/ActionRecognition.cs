using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Util;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    public class ActionRecognition : IDisposable
    {
        /// <summary>
        /// 模型的文件夹
        /// </summary>
        public string DirPath { get; set; }

        /// <summary>
        /// 要识别的动作模型列表
        /// </summary>
        public ActionModel Model { get; set; }

        /// <summary>
        /// 动作模型文件路径列表
        /// </summary>
        public List<string> ModeFilePathList { get; set; }

        #region 单例

        private static ActionRecognition _instance = null;
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 用单例的模式获取动作识别类<see cref="ActionRecognition"/>
        /// </summary>
        /// <returns></returns>
        public static ActionRecognition GetActionRecognition()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new ActionRecognition();
                }
            }

            return _instance;
        }

        #endregion

        private ActionRecognition()
        {
            ModeFilePathList = new List<string>();
            DirPath = GlobalConfig.ActionModelDirPath;
        }

        /// <summary>
        /// 加载指定文件夹里的模型文件路径
        /// </summary>
        /// <param name="dirPath">模型文件的路径</param>
        public void LoadModelDir(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                LogUtil.Debug(this, $"指定的模型文件夹:{dirPath} 不存在！");
                return;
            }

            DirPath = dirPath;
            string[] filePathArr = Directory.GetFiles(dirPath);
            foreach (string filePath in filePathArr)
            {
                string extension = Path.GetExtension(filePath);
                if (extension == ".actionmodel" && !ModeFilePathList.Contains(filePath))
                    ModeFilePathList.Add(filePath);
            }
        }

        /// <summary>
        /// 加载模型文件
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadModelFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                LogUtil.Debug(this, $"指定的模型文件路径为空!");
                return;
            }

            if (!File.Exists(filePath))
            {
                LogUtil.Debug(this, $"指定的模型文件:{filePath} 不存在！");
                return;
            }

            try
            {
                string xmlStr = File.ReadAllText(filePath);
                Model = LoadModelFromString(xmlStr);
            }
            catch (Exception ex)
            {
                LogUtil.Error(this, ex);
            }
        }

        /// <summary>
        /// 加载模型XML
        /// </summary>
        /// <param name="modelXmlStr"></param>
        public ActionModel LoadModelFromString(string modelXmlStr)
        {
            if (string.IsNullOrEmpty(modelXmlStr))
            {
                LogUtil.Debug(this, "指定的模型XML字符串不能为空！");
                return null;
            }

            ActionModel action = XmlUtil.Deserialize<ActionModel>(modelXmlStr);
            return action;
        }

        /// <summary>
        /// 姿态识别
        /// </summary>
        /// <returns>返回识别的结果 <see cref=" List&lt;RecognitionResult&gt; "/></returns>
        public RecognitionResult Identification(IReadOnlyDictionary<JointType, Joint> joints3)
        {
            RecognitionResult result = new RecognitionResult() {InfoMessages = ""};
            if (Model != null)
            {
                if (!Model.IsCompared)
                    Model.Compared(joints3, out result);
                else
                    result.InfoMessages = Model.ActionName + "已完成";
            }
            else
            {
                result.InfoMessages = "没有动作模型";
            }

            return result;
        }

        public void Dispose()
        {
            Model?.Dispose();
            ModeFilePathList?.Clear();
            ModeFilePathList = null;
        }
    }
}