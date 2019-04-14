/*
 * Description:             工具静态类
 * Author:                  tanghuan
 * Create Date:             2018/09/01
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    /// <summary>
    /// 工具静态类
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// 本机打开特定目录(暂时只用于Windows)
        /// </summary>
        /// <param name="folderPath"></param>
        public static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(folderPath, "explorer.exe");
                Process.Start(startInfo);
            }
            else
            {
                Console.WriteLine(string.Format("{0} Directory does not exist!", folderPath));
            }
        }

        /// <summary>
        /// 检查指定目录是否存在，不存在创建一个
        /// </summary>
        public static void CheckOrCreateSpecificFolder(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }
        }

        /// <summary>
        /// 无论目录是否存在都删除所有文件重新创建一个目录
        /// </summary>
        public static void RecreateSpecificFolder(string folderpath)
        {
            if (Directory.Exists(folderpath))
            {
                Directory.Delete(folderpath, true);
            }
            Directory.CreateDirectory(folderpath);
        }

        /// <summary>
        /// 获取文件的目录名字
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string GetFileFolderName(string filepath)
        {
            return Path.GetFileName(Path.GetDirectoryName(filepath));
        }
    }
}
