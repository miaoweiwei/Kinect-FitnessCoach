using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FitnessCoach.BoneNode;
using FitnessCoach.Core;
using FitnessCoach.Util;
using Microsoft.Kinect;

namespace TestWinForm
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private void TestFrom_Load(object sender, EventArgs e)
        {
            SRecognition();
        }

        //朗读
        private void button1_Click(object sender, EventArgs e)
        {
            SpeechHelp speech = SpeechHelp.GetInstance();
            speech.Speak("你好呀，In a member-led church, all of the members in the congregation generall", true);
        }

        //序列化
        private void button2_Click(object sender, EventArgs e)
        {
            AttitudeModel bodyJoint = new AttitudeModel();
            bodyJoint.AttitudeName = "RaiseLeftHand";
            bodyJoint.JointAngles = new List<JointAngle>()
            {
                //左臂的关节节点
                new JointAngle() {Name = JointType.HandLeft, Angle = 180},
                new JointAngle() {Name = JointType.WristLeft, Angle = 180},
                new JointAngle() {Name = JointType.ElbowLeft, Angle = 180},
                new JointAngle() {Name = JointType.ShoulderLeft, Angle = 180},
            };

            bodyJoint.KeyBones = new List<KeyBone>()
            {
                new KeyBone()
                {
                    Name = Bone.BigArmLeft,
                    Vector = new CameraSpacePoint() {X = -50, Y = 0, Z = 0},
                    AngleX = 0,
                    AngleY = 90,
                    AngleZ = 90
                },
            };
            string serializationStr = XmlUtil.Serializer(bodyJoint);


            ActionModel action = new ActionModel()
            {
                ActionName = "抬手",
                ActionFrameList = new List<ActionFrame>()
                {
                   new ActionFrame(){Nmae = "抬手",Joints = new Dictionary<JointType, Joint>()
                   {
                       {JointType.Head,new Joint(){JointType = JointType.Head,Position = new CameraSpacePoint(){X = 10,Y=100}} },
                       {JointType.Neck,new Joint(){JointType = JointType.Neck,Position = new CameraSpacePoint(){X = 10,Y=120}} },
                       {JointType.ShoulderLeft,new Joint(){JointType = JointType.ShoulderLeft,Position = new CameraSpacePoint(){X = 10,Y=150}} },
                       {JointType.SpineMid,new Joint(){JointType = JointType.SpineMid,Position = new CameraSpacePoint(){X = 10,Y=180}} },
                   }},
                   new ActionFrame(){Nmae = "踢腿",Joints = new Dictionary<JointType, Joint>()
                   {
                       {JointType.Head,new Joint(){JointType = JointType.Head,Position = new CameraSpacePoint(){X = 10,Y=100}} },
                       {JointType.Neck,new Joint(){JointType = JointType.Neck,Position = new CameraSpacePoint(){X = 10,Y=120}} },
                       {JointType.ShoulderLeft,new Joint(){JointType = JointType.ShoulderLeft,Position = new CameraSpacePoint(){X = 10,Y=150}} },
                       {JointType.SpineMid,new Joint(){JointType = JointType.SpineMid,Position = new CameraSpacePoint(){X = 10,Y=180}} },
                   }}
                }
            };
            serializationStr = XmlUtil.Serializer(action);

            //action.KeyFrameAttitudeModelList = new List<AttitudeModel>(){ bodyJoint };

            AttitudeModel obj = XmlUtil.Deserialize<AttitudeModel>(serializationStr);
        }

        private SpeechRecognitionEngine recognizer;

        public void SRecognition() //创建关键词语列表  
        {
            //创建中文识别器
            recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("zh-CN"));

            //----------------
            //初始化命令词
            Choices conmmonds = new Choices();
            //添加命令词
            conmmonds.Add(new string[] {"红色", "黑色", "白色"});
            //初始化命令词管理
            GrammarBuilder gBuilder = new GrammarBuilder();
            //将命令词添加到管理中
            gBuilder.Append(conmmonds);
            //实例化命令词管理
            Grammar grammar = new Grammar(gBuilder);
            //-----------------

            //创建并加载听写语法(添加命令词汇识别的比较精准)
            recognizer.LoadGrammar(grammar);
            //为语音识别事件添加处理程序。
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            //将输入配置到语音识别器。
            recognizer.SetInputToDefaultAudioDevice();
            //启动异步，连续语音识别。
            //recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Debug.WriteLine(e.Result.Text);
        }

        //语音识别
        private void btnSpeechRecognition1_Click(object sender, EventArgs e)
        {
            System.Speech.Recognition.RecognitionResult result = recognizer.Recognize();
            // Console.WriteLine(result.Text);
            //启动异步，连续语音识别。
            //recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void btnSpeechRecognition2_Click(object sender, EventArgs e)
        {
            recognizer.RecognizeAsyncStop();
        }
    }
}