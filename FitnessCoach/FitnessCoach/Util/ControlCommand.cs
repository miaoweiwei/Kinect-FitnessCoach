using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCoach.BoneNode;
using Microsoft.Kinect;

namespace FitnessCoach.Util
{
    /// <summary>
    /// 权限控制
    /// </summary>
    public class ControlCommand
    {
        public delegate void SwitchLeaderHandler(Body body, bool isLeader);

        public delegate void LeftHandCloseHandler(Body body);
        public delegate void RightHandCloseHandler(Body body);
        public delegate void HandCloseHandler(Body body);

        public event SwitchLeaderHandler SwitchLeaderEvent;

        public event LeftHandCloseHandler LeftHandCloseEvent;
        public event RightHandCloseHandler RightHandCloseEvent;
        public event HandCloseHandler HandCloseEvent;

        public delegate void LeftHandOpenHandler(Body body);
        public delegate void RightHandOpenHandler(Body body);
        public delegate void HandOpenHandler(Body body);

        public event LeftHandOpenHandler LeftHandOpenEvent;
        public event RightHandOpenHandler RightHandOpenEvent;
        public event HandOpenHandler HandOpenEvent;

        /// <summary> 当前有权限的人 </summary>
        public ulong LeaderId = 0uL;

        private Dictionary<ulong, bool> _mmStateDic;
        private Dictionary<Hand, HandState> _handStateDic;

        private const float PalmAngle = 160;

        private static ControlCommand _instance = null;
        private static readonly object SyncRoot = new object();

        private ControlCommand()
        {
            _mmStateDic = new Dictionary<ulong, bool>();
            _handStateDic = new Dictionary<Hand, HandState>()
            {
                {Hand.LeftHand, HandState.Unknown},
                {Hand.RightHand, HandState.Unknown}
            };
        }

        public static ControlCommand GetControlCommand()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new ControlCommand();
                }
            }

            return _instance;
        }

        /// <summary> 权限切换 </summary>
        public void SwitchLeader(Body body)
        {
            var flag = false;
            var headY = body.Joints[JointType.Head].Position.Y;
            var headX = body.Joints[JointType.Head].Position.X;
            if (body.Joints[JointType.HandRight].Position.Y > headY &&
                body.Joints[JointType.HandRight].Position.X > headX &&
                body.Joints[JointType.HandLeft].Position.Y > headY &&
                body.Joints[JointType.HandLeft].Position.X < headX)
                flag = true;
            if (!_mmStateDic.ContainsKey(body.TrackingId))
                _mmStateDic[body.TrackingId] = false;

            if (_mmStateDic[body.TrackingId])
            {
                if (!flag)
                    _mmStateDic[body.TrackingId] = false;
                else if (body.HandLeftState == HandState.Closed && body.HandRightState == HandState.Closed)
                {
                    if (LeaderId != 0 && LeaderId == body.TrackingId)
                        LeaderId = 0uL;
                    else
                        LeaderId = body.TrackingId;
                    OnSwitchLeaderEvent(body, LeaderId == body.TrackingId);
                    _mmStateDic[body.TrackingId] = false;
                }
            }
            else
            {
                if (flag && body.HandLeftState == HandState.Open && body.HandRightState == HandState.Open)
                    _mmStateDic[body.TrackingId] = true; //获取权限动作的开始
                else
                    _mmStateDic[body.TrackingId] = false;
            }
        }

        /// <summary>
        /// 处理有关事件,只有有权限的人才会触发事件
        /// </summary>
        /// <param name="body"></param>
        public void HandClosedEvent(Body body)
        {
            //关节的角度
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(body.Joints);
            float leftAngle = jointAngles.First(o => o.Name == JointType.HandLeft).Angle;
            Debug.WriteLine("左手握拳," + leftAngle);
            if (body.TrackingId != LeaderId) return;

            if (body.HandLeftState == HandState.Closed && body.HandLeftConfidence == TrackingConfidence.High &&
                leftAngle < PalmAngle)
            {
                if (_handStateDic[Hand.LeftHand] != HandState.Closed)
                {
                    _handStateDic[Hand.LeftHand] = HandState.Closed;
                    OnLeftHandClosedEvent(body);
                }
            }
            else
                _handStateDic[Hand.LeftHand] = body.HandLeftState;

            float rightAngle = jointAngles.First(o => o.Name == JointType.HandRight).Angle;
            if (body.HandRightState == HandState.Closed && body.HandRightConfidence == TrackingConfidence.High &&
                rightAngle < PalmAngle)
            {
                if (_handStateDic[Hand.RightHand] != HandState.Closed)
                {
                    _handStateDic[Hand.RightHand] = HandState.Closed;
                    OnRightHandClosedEvent(body);
                }
            }
            else
                _handStateDic[Hand.RightHand] = body.HandRightState;
        }

        protected virtual void OnLeftHandClosedEvent(Body body)
        {
            Debug.WriteLine("左手握拳,");
            SpeechHelp.GetInstance().Speak("左手握拳", true);
            LeftHandCloseEvent?.Invoke(body);
            OnHandClosedEvent(body);
        }

        protected virtual void OnRightHandClosedEvent(Body body)
        {
            Debug.WriteLine("右手握拳");
            SpeechHelp.GetInstance().Speak("右手握拳", true);
            RightHandCloseEvent?.Invoke(body);
            OnHandClosedEvent(body);
        }

        protected virtual void OnHandClosedEvent(Body body)
        {
            HandCloseEvent?.Invoke(body);
        }

        protected virtual void OnSwitchLeaderEvent(Body body, bool isLeader)
        {
            SwitchLeaderEvent?.Invoke(body, isLeader);

            Debug.WriteLine($"授权变更的人：{body.TrackingId}；变更结果：{isLeader}");
        }
    }

    internal enum Hand
    {
        LeftHand,
        RightHand
    }
}