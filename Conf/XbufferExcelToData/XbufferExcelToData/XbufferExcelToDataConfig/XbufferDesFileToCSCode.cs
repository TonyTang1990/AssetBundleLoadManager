/*
 * Description:             自动化根据数据结构文件生成CS对应代码的静态单例类
 * Author:                  tanghuan
 * Create Date:             2018/09/02
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
    /// 自动化根据数据结构文件生成CS对应代码的静态单例类
    /// </summary>
    public class XbufferDesFileToCSCode : SingletonTemplate<XbufferDesFileToCSCode>
    {
        /// <summary> 数据结构定义文件目录 /// </summary>
        public string DesFileFolderPath { get; private set; }

        /// <summary> Xbuffer模板文件目录 /// </summary>
        public string TemplateFolderPath { get; private set; }

        /// <summary> CS类代码生成目录 /// </summary>
        public string CSClassCodeFolderPath { get; private set; }

        /// <summary> CS序列化代码生成目录 /// </summary>
        public string CSBufferCodeFolderPath { get; private set; }

        public XbufferDesFileToCSCode()
        {
            DesFileFolderPath = string.Empty;
            CSClassCodeFolderPath = string.Empty;
            CSBufferCodeFolderPath = string.Empty;
        }

        /// <summary>
        /// 配置相关目录路径
        /// </summary>
        /// <param name="desfilefolderpath">数据结构文件目录</param>
        /// <param name="templatefolderpath">Xbuffer模本文件目录</param>
        /// <param name="csclassfolderpath">CS类代码文件生成目录</param>
        /// <param name="csbufferfolderpath">CS序列化代码文件生成目录</param>
        public void configFolderPath(string desfilefolderpath, string templatefolderpath, string csclassfolderpath, string csbufferfolderpath)
        {
            DesFileFolderPath = desfilefolderpath;
            TemplateFolderPath = TemplateFolderPath;
            CSClassCodeFolderPath = csclassfolderpath;
            CSBufferCodeFolderPath = csbufferfolderpath;
        }

        /// <summary>
        /// 将所有数据结构文件生成对应的所有Xbuffer相关代码(含对应类代码以及序列化相关代码)
        /// </summary>
        /// <returns></returns>
        public bool writeAllDesFileToCSCode()
        {
            Utilities.RecreateSpecificFolder(CSClassCodeFolderPath);
            Utilities.RecreateSpecificFolder(CSBufferCodeFolderPath);
            Process process = new Process();
            string csclasspara = string.Format("input={0} template=\"{1}/csharp_class.ftl\" output_dir=\"{2}\" suffix=\".cs\"",
                                                XbufferExcelExportConfig.Singleton.ExportConfigInfo.DesFileOutputPath,
                                                XbufferExcelExportConfig.Singleton.ExportConfigInfo.TemplatePath,
                                                XbufferExcelExportConfig.Singleton.ExportConfigInfo.CSClassCodeOutputPath);
            string csbufferpara = string.Format("input={0} template=\"{1}csharp_buffer.ftl\" output_dir=\"{2}\" suffix=\"Buffer.cs\"",
                                                XbufferExcelExportConfig.Singleton.ExportConfigInfo.DesFileOutputPath,
                                                XbufferExcelExportConfig.Singleton.ExportConfigInfo.TemplatePath,
                                                XbufferExcelExportConfig.Singleton.ExportConfigInfo.CSBufferCodeOutputPath);
            startXbufferCSClassProcess(process, csclasspara);
            startXbufferCSBufferProcess(process, csbufferpara);
            return true;
        }

        /// <summary>
        /// 开启Xbuffer生成CS类代码的进程
        /// </summary>
        /// <param name="process"></param>
        /// <param name="parameters"></param>
        private void startXbufferCSClassProcess(Process process, string parameters)
        {
            ProcessStartInfo csclassprocessstartinfo = new ProcessStartInfo(ConstValue.XbufferParserExePath, parameters);
            process.StartInfo = csclassprocessstartinfo;
            process.Start();
            while (!process.HasExited)
            {
                process.WaitForExit();
            }
            int returnValue = process.ExitCode;
        }

        /// <summary>
        /// 开启Xbuffer生成CS序列化代码的进程
        /// </summary>
        /// <param name="process"></param>
        /// <param name="parameters"></param>
        private void startXbufferCSBufferProcess(Process process, string parameters)
        {
            ProcessStartInfo csclassprocessstartinfo = new ProcessStartInfo(ConstValue.XbufferParserExePath, parameters);
            process.StartInfo = csclassprocessstartinfo;
            process.Start();
            while (!process.HasExited)
            {
                process.WaitForExit();
            }
            int returnValue = process.ExitCode;
        }
    }
}
