using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Util;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    /// <summary>
    /// 姿态识别类
    /// </summary>
    public class AttitudeRecognition
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("AttitudeRecognition");

        /// <summary>
        /// 模型的文件夹
        /// </summary>
        public string DirPath { get; set; } = GlobalConfig.ModelDirPath;

        /// <summary>
        /// 要识别的模型列表
        /// </summary>
        public List<AttitudeModel> ModelList { get; set; }

        #region 单例

        private static AttitudeRecognition _instance = null;
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 用单例的模式获取姿态识别类<see cref="AttitudeRecognition"/>
        /// </summary>
        /// <returns></returns>
        public static AttitudeRecognition GetAttitudeRecognition()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new AttitudeRecognition();
                }
            }

            return _instance;
        }

        #endregion

        private AttitudeRecognition()
        {
            ModelList = new List<AttitudeModel>();
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
                LoadModelFromFile(filePath);
            }
        }

        /// <summary>
        /// 加载模型文件
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadModelFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Debug($"指定的模型文件:{filePath} 不存在！");
                return;
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            try
            {
                string xmlStr = File.ReadAllText(filePath);
                LoadModelFromString(xmlStr);
            }
            catch (Exception ex)
            {
                Log.Error($"模型加载失败：{ex.Message};错误地址：{ex.StackTrace.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim()}");
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

            AttitudeModel attitude = XmlUtil.Deserialize<AttitudeModel>(modelXmlStr);
            if (this.ModelList.Exists(o => o.AttitudeName == attitude.AttitudeName))
            {
                int index = this.ModelList.FindIndex(o => o.AttitudeName == attitude.AttitudeName);
                this.ModelList[index] = attitude;
            }
            else
                this.ModelList.Add(attitude);
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
            foreach (AttitudeModel model in ModelList)
            {
                //if (model.Compared(jointAngles, keyBones, out RecognitionResult result))
                //{
                //    resultList.Add(result);
                //}
                model.Compared(jointAngles, keyBones, out RecognitionResult result);
                resultList.Add(result);

                //KeyBone k = keyBones.First(o => o.Name == model.KeyBones[0].Name);
                //Debug.WriteLine($"识别结果:{k.Name},X:{k.AngleX},Y:{k.AngleY},Z:{k.AngleZ}");
                //Debug.WriteLine($"提示信息:{msg}");
            }

            return resultList;
        }
    }
}