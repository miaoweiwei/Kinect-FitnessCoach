using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FitnessCoach.Util
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public static class SerializationHelp
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="xml">要序列号的字符串</param>
        /// <returns>返回指定对象类型的对象</returns>
        public static T Deserialize<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            StringReader sr = new StringReader(xml);
            T obj = (T)xs.Deserialize(sr);
            sr.Close();
            sr.Dispose();
            return obj;
        }

        /// <summary>
        /// 序列化,没有无参的构造函数的对象不可以序列化，基础类型都不可以要放在对象里
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns></returns>
        public static string Serializer<T>(T t)
        {
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add(string.Empty, string.Empty);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            StringWriter sw = new StringWriter();
            xs.Serialize(sw, t, xsn);
            string str = sw.ToString();
            sw.Close();
            sw.Dispose();
            return str;
        }
    }
}
