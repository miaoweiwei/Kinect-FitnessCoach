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
using FitnessCoach.Util;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    /// <summary>
    /// 姿态识别类
    /// </summary>
    public class AttitudeRecognition
    {
        /// <summary>
        /// 模型的文件夹
        /// </summary>
        public string DirPath { get; set; }

        /// <summary>
        /// 要识别的模型列表
        /// </summary>
        public List<AttitudeModel> ModelList { get; set; }

        /// <summary>
        /// 姿态识别类
        /// </summary>
        /// <param name="dirPath">模型文件的路径</param>
        public AttitudeRecognition(string dirPath)
        {
            DirPath = dirPath;
            ModelList = new List<AttitudeModel>();
            LoadModel();
        }

        /// <summary>
        /// 加载指定<see cref="DirPath"/>文件夹里的模型模型
        /// </summary>
        public void LoadModel()
        {
            LoadModel(DirPath);
        }

        /// <summary>
        /// 加载指定文件夹里的模型
        /// </summary>
        /// <param name="dirPath"></param>
        public void LoadModel(string dirPath)
        {
            string[] filePathArr = Directory.GetFiles(dirPath);
            foreach (string filePath in filePathArr)
            {
                LoadModelFromFile(filePath);
            }
        }

        public void LoadModelFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;
            string xmlStr = File.ReadAllText(filePath);
            LoadModelFromString(xmlStr);
        }

        public void LoadModelFromString(string modelXmlStr)
        {
            AttitudeModel attitude = XmlUtil.Deserialize<AttitudeModel>(modelXmlStr);
            this.ModelList.Add(attitude);
        }

        /// <summary>
        /// 姿态识别
        /// </summary>
        /// <returns></returns>
        public string Identification(IReadOnlyDictionary<JointType, Joint> joints3)
        {
            List<KeyBone> keyBones = Skeleton.GetBodyAllKeyBones(joints3);
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);
            StringBuilder compareResults = new StringBuilder();
            StringBuilder promptMsg = new StringBuilder();

            foreach (AttitudeModel model in ModelList)
            {
                if (model.Compared(jointAngles, keyBones, out string msg))
                {
                    compareResults.Append(model.AttitudeName + ",");
                    promptMsg.Append(msg + ",");
                }

                KeyBone k = keyBones.First(o => o.Name == model.KeyBones[0].Name);
                Debug.WriteLine($"识别结果:{k.Name},X:{k.AngleX},Y:{k.AngleY},Z:{k.AngleZ}");
                Debug.WriteLine($"提示信息:{msg}");
            }

            compareResults.Remove(compareResults.Length - 1, 1);
            promptMsg.Remove(promptMsg.Length - 1, 1);

            Debug.WriteLine(compareResults);
            compareResults.Append("#*#" + promptMsg);
            return compareResults.ToString();
        }
    }
}