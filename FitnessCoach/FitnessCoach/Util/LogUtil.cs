using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FitnessCoach.Util
{
    public static class LogUtil
    {
        public static void Error(object obj, Exception ex)
        {
            Error(obj.GetType(), ex);
        }

        public static void Error<T>(Exception ex)
        {
            Error(typeof(T), ex);
        }

        public static void Error(Type obj, Exception ex)
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(obj.FullName);
            Log.Error(
                $"程序发生错误：{ex.Message};错误地址：{ex.StackTrace.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim()}");
        }

        public static void Debug(object obj, string msg)
        {
            Debug(obj.GetType(), msg);
        }

        public static void Debug<T>(string msg)
        {
            Debug(typeof(T), msg);
        }

        public static void Debug(Type obj, string msg)
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(obj.FullName);
            Log.Debug(msg);
        }

        public static void Info(object obj, string msg)
        {
            Info(obj.GetType(), msg);
        }

        public static void Info<T>(string msg)
        {
            Info(typeof(T), msg);
        }

        public static void Info(Type obj, string msg)
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(obj.FullName);
            Log.Info(msg);
        }
    }
}