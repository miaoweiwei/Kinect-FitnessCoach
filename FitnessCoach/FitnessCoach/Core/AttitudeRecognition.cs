using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCoach.Core
{
    /// <summary>
    /// 姿态识别类
    /// </summary>
    public class AttitudeRecognition
    {
        public string DirPath { get; set; }
        public List<AttitudeModel> ModelList;


        /// <summary>
        /// 姿态识别类
        /// </summary>
        /// <param name="dirPath">模型文件的路径</param>
        public AttitudeRecognition(string dirPath)
        {
        }
    }
}