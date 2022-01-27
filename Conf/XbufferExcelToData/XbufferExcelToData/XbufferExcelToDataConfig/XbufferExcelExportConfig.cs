/*
 * Description:             导表工具相关配置单例类
 * Author:                  tanghuan
 * Create Date:             2018/09/01
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XbufferExcelToData
{
    /// <summary>
    /// 导表工具相关配置单例类
    /// </summary>
    public class XbufferExcelExportConfig : SingletonTemplate<XbufferExcelExportConfig>
    {
        /// <summary>
        /// XML配置文件全路径
        /// </summary>
        public string XMLConfigFileFullPath { get; private set; }

        /// <summary>
        /// 导出配置数据
        /// </summary>
        public ExportConfig ExportConfigInfo { get; private set; }

        public XbufferExcelExportConfig()
        {
            XMLConfigFileFullPath = "./ExportConfig.xml";
            ExportConfigInfo = null;
        }

        /// <summary>
        /// 加载导出配置数据
        /// </summary>
        public bool LoadExportConfigData()
        {
            if (File.Exists(XMLConfigFileFullPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(ExportConfig));
                FileStream fs = new FileStream(XMLConfigFileFullPath, FileMode.Open);
                ExportConfigInfo = (ExportConfig)ser.Deserialize(fs);
                fs.Close();
                return ExportConfigInfo != null;
            }
            else
            {
                Console.WriteLine(string.Format("导出配置文件不存在 : {0}", XMLConfigFileFullPath));
                return false;
            }
        }
    }
}
