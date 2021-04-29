using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Pws
{
    /// <summary>
    /// .ini 文件
    /// </summary>
    public class IniFile
    {
        public string FilePath { get; private set; }

        /// <summary>
        /// 指定文件
        /// </summary>
        /// <param name="path"></param>
        public IniFile(string path)
        {
            FilePath = path;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public string Get(string section, string key, string defaultValue=null, int size=1024)
        {
            StringBuilder result = new StringBuilder(size);
            GetPrivateProfileString(section, key, defaultValue, result, size, FilePath);
            return result.ToString();
        }

        /// <summary>
        /// 获取布尔值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            return bool.Parse(Get(section, key, defaultValue.ToString()));
        }

        /// <summary>
        /// 获取整型值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetInt(string section, string key, int defaultValue = 0)
        {
            return int.Parse(Get(section, key, defaultValue.ToString()));
        }

        /// <summary>
        /// 获取数值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public double GetNumber(string section, string key, double defaultValue = 0.0)
        {
            return double.Parse(Get(section, key, defaultValue.ToString()));
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public long Set(string section, string key, string data)
        {
            return WritePrivateProfileString(section, key, data, FilePath);
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string data, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder result, int size, string filePath);
    }
}
