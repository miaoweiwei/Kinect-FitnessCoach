using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Util;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    public class ActionRecognition
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("ActionRecognition");

        /// <summary>
        /// 模型的文件夹
        /// </summary>
        public string DirPath { get; set; } = GlobalConfig.ActionModelDirPath;

        /// <summary>
        /// 要识别的动作模型列表
        /// </summary>
        public List<ActionModel> ModelList { get; set; }

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
            ModelList = new List<ActionModel>();
            this.LoadModel(DirPath);
        }

        /// <summary>
        /// 加载指定文件夹里的模型
        /// </summary>
        /// <param name="dirPath">模型文件的路径</param>
        public void LoadModel(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Log.Debug($"指定的模型文件夹:{dirPath} 不存在！");
                return;
            }

            string[] filePathArr = Directory.GetFiles(dirPath);
            foreach (string filePath in filePathArr)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                int index = fileName.LastIndexOf('_');
                if (fileName.Substring(index + 1) == "Action")
                    LoadModelFromFile(filePath);
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
                Log.Debug($"指定的模型文件路径为空！");
                return;
            }

            if (!File.Exists(filePath))
            {
                Log.Debug($"指定的模型文件:{filePath} 不存在！");
                return;
            }

            try
            {
                string xmlStr = File.ReadAllText(filePath);
                LoadModelFromString(xmlStr);
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"模型加载失败：{ex.Message};错误地址：{ex.StackTrace.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim()}");
            }
        }

        /// <summary>
        /// 加载模型XML
        /// </summary>
        /// <param name="modelXmlStr"></param>
        public void LoadModelFromString(string modelXmlStr)
        {
            if (string.IsNullOrEmpty(modelXmlStr))
            {
                Log.Debug($"指定的模型XML字符串不能为空！");
                return;
            }

            ActionModel action = XmlUtil.Deserialize<ActionModel>(modelXmlStr);
            if (this.ModelList.Exists(o => o.ActionName == action.ActionName))
            {
                int index = this.ModelList.FindIndex(o => o.ActionName == action.ActionName);
                this.ModelList[index] = action;
            }
            else
                this.ModelList.Add(action);
        }

        /// <summary>
        /// 姿态识别
        /// </summary>
        /// <returns>返回识别的结果 <see cref=" List&lt;RecognitionResult&gt; "/></returns>
        public List<RecognitionResult> Identification(IReadOnlyDictionary<JointType, Joint> joints3)
        {
            List<KeyBone> keyBones = Skeleton.GetBodyAllKeyBones(joints3);
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);

            List<RecognitionResult> resultList = new List<RecognitionResult>();
            foreach (ActionModel model in ModelList)
            {
                model.Compared(jointAngles, keyBones, out RecognitionResult result);
                resultList.Add(result);
            }

            return resultList;
        }
    }
}