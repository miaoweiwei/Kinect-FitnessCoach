using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Speech.Synthesis; //用于生成响应的事件
using System.Speech.Synthesis.TtsEngine;
using System.Speech.Recognition;

namespace FitnessCoach.Util
{
    /// <summary>
    /// 语音模块
    /// </summary>
    public class SpeechHelp
    {
        private SpeechSynthesizer _speech;

        /// <summary>
        /// 朗读音量 [范围 0 ~ 100] 
        /// </summary>
        public int Volume
        {
            get { return _speech.Volume; }
            set { _speech.Volume = value; }
        }

        /// <summary>
        /// 设置朗读频率 [范围  -10 ~ 10] 
        /// </summary>
        public int Rate
        {
            get { return _speech.Rate; }
            set { _speech.Rate = value; }
        }

        /// <summary>
        /// 朗读状态
        /// </summary>
        public SynthesizerState SpeechState
        {
            get { return _speech.State; }
        }

        #region 单例

        private static SpeechHelp _instance = null;
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 单例获取语音模块类<see cref="FitnessCoach.Util.SpeechHelp"/>
        /// </summary>
        /// <returns></returns>
        public static SpeechHelp GetInstance()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new SpeechHelp();
                }
            }

            return _instance;
        }

        private SpeechHelp()
        {
            // 在此处初始化
            _speech = new SpeechSynthesizer();
            ReadOnlyCollection<InstalledVoice> installedVoices = _speech.GetInstalledVoices();
            if (installedVoices.Count <= 0)
            {
                string voiceName = installedVoices.First(o => o.VoiceInfo.Culture.Name == "zh-CN").VoiceInfo.Name;
                _speech.SelectVoice(voiceName);
            }

            _speech.Rate = 0;
            _speech.Volume = 100;
            _speech.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Teen, 3, CultureInfo.CurrentCulture);
            _speech.SpeakStarted += Speech_SpeakStarted;
            _speech.StateChanged += _speech_StateChanged;
            _speech.SpeakCompleted += Speech_SpeakCompleted; //异步朗读的时候才触发
        }

        #endregion


        private void Speech_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            Debug.WriteLine("开始朗读：" + e.Prompt);
        }

        private void _speech_StateChanged(object sender, System.Speech.Synthesis.StateChangedEventArgs e)
        {
            Debug.WriteLine("状态改变之前：" + e.PreviousState.ToString());
            Debug.WriteLine("状态改变之后：" + e.State.ToString());
        }

        private void Speech_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            SpeechSynthesizer s = sender as SpeechSynthesizer;
            if (e.Prompt.IsCompleted)
            {
                Debug.WriteLine("朗读完成!");
            }
        }

        /// <summary>
        /// 朗读，异步的时候触发朗读完成，同步的时候不触发
        /// </summary>
        /// <param name="textToSpeak"></param>
        /// <param name="isAsync"></param>
        public void Speak(string textToSpeak, bool isAsync = false)
        {
            if (isAsync)
            {
                //朗读之前取消之前没有朗读完或者没有朗读的队列
                _speech.SpeakAsyncCancelAll();
                _speech.SpeakAsync(textToSpeak);
            }
            else
                _speech.Speak(textToSpeak);
        }
    }
}