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
            if(File.Exists(XMLConfigFileFullPath))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(XMLConfigFileFullPath);
                var rootnode = xmldoc.DocumentElement;
                var rootnodename = rootnode.Name;
                var reflecttype = this.GetType().Assembly.GetType("XbufferExcelToData." + rootnodename);
                if(reflecttype ==  typeof(ExportConfig))
                {
                    ExportConfigInfo = new ExportConfig();
                    var childnodes = rootnode.ChildNodes;
                    for(int i = 0, length = childnodes.Count; i < length; i++)
                    {
                        var property = ExportConfigInfo.GetType().GetProperty(childnodes[i].Name);
                        if(property != null)
                        {
                            // 支持配置相对路径
                            var fullpath = Path.GetFullPath(childnodes[i].InnerText);
                            fullpath = fullpath.Replace("\\", "/");
                            property.SetValue(ExportConfigInfo, fullpath);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("不支持的属性配置 : {0}", childnodes[i].Name));
                        }
                    }
                    ExportConfigInfo.printOutAllInfo();
                    return true;
                }
                else
                {
                    Console.WriteLine(string.Format("找不到配置的类型数据信息 : {0}", rootnodename));
                    Console.WriteLine("当前只支持ExportConfig类型信息配置");
                    return false;
                }
            }
            else
            {
                Console.WriteLine(string.Format("导出配置文件不存在 : {0}", XMLConfigFileFullPath));
                return false;
            }
        }
    }
}
