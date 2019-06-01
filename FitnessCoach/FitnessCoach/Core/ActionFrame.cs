using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    public class ActionFrame
    {
        public string Nmae;
        public Dictionary<JointType, Joint> Joints;
    }
}