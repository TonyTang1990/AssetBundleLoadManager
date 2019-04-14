/*
 * Description:             模板解析单例类
 * Author:                  tanghuan
 * Create Date:             2018/09/03
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XbufferExcelToData
{
    /// <summary>
    /// 模板解析单例类
    /// </summary>
    public class XbufferTemplateToCSCode : SingletonTemplate<XbufferTemplateToCSCode>
    {
        /// <summary> 模板文件目录 /// </summary>
        public string TempalteFolderPath { get; private set; }

        /// <summary> 模板CS代码生成目录 /// </summary>
        public string TemplateCSOutputPath { get; private set; }

        /// <summary> Excel数据信息列表 /// </summary>
        private List<ExcelInfo> mExcelInfoList;

        /// <summary> 表格容器模板文件名 /// </summary>
        private const string mExcelContainerTemplateFileName = "excelContainer.ftl";

        /// <summary> 数据统一加载模板文件名 /// </summary>
        private const string mGameDataManagerTemplateFileName = "GameDataManager.ftl";

        /// <summary> 模板实例对象 /// </summary>
        private TTemplate mTemplateInstance;

        public XbufferTemplateToCSCode()
        {
            mTemplateInstance = new TTemplate();
        }

        /// <summary>
        /// 配置模板相关信息
        /// </summary>
        /// <param name="templatefolderpath">模板目录</param>
        /// <param name="templatecsoutputpath">模板CS代码输出目录</param>
        /// <param name="excelinfolist">excel信息列表</param>
        public void configTemplateInfo(string templatefolderpath, string templatecsoutputpath, List<ExcelInfo> excelinfolist)
        {
            TempalteFolderPath = templatefolderpath;
            TemplateCSOutputPath = templatecsoutputpath;
            mExcelInfoList = excelinfolist;
        }

        /// <summary>
        /// 解析表格容器模板
        /// </summary>
        /// </summary>
        public void parseExcelContainerTemplate()
        {
            if(!Directory.Exists(TempalteFolderPath))
            {
                Console.WriteLine(string.Format("模板文件目录不存在 : {0}", TempalteFolderPath));
                Console.WriteLine("无法正常解析表格容器模板");
                return;
            }
            else if(!File.Exists(TempalteFolderPath + mExcelContainerTemplateFileName))
            {
                Console.WriteLine(string.Format("模板文件不存在 : {0}{1}", TempalteFolderPath, mExcelContainerTemplateFileName));
                Console.WriteLine("无法正常解析表格容器模板");
                return;
            }
            else
            {
                Utilities.CheckOrCreateSpecificFolder(TemplateCSOutputPath);

                var templatefilefullpath = TempalteFolderPath + mExcelContainerTemplateFileName;
                var templatecontent = File.ReadAllText(templatefilefullpath);
                // 为每一个excel生成单独的excelContainer.cs
                foreach(var excelinfo in mExcelInfoList)
                {
                    // 重置模板类容
                    mTemplateInstance.resetContent(templatecontent);
                    // 替换表格加载管理里的类名
                    mTemplateInstance.setValue("#CLASS_NAME#", excelinfo.ExcelName);
                    // 输出生成内容到文件
                    var outputfilefullpath = TemplateCSOutputPath + excelinfo.ExcelName + "Container.cs";
                    File.WriteAllText(outputfilefullpath, mTemplateInstance.getContent());
                }

                mTemplateInstance.resetContent(string.Empty);
            }
        }

        /// <summary>
        /// 解析数据加载管理模板
        /// </summary>
        /// </summary>
        public void parseGameDataManagerTemplate()
        {
            if (!Directory.Exists(TempalteFolderPath))
            {
                Console.WriteLine(string.Format("模板文件目录不存在 : {0}", TempalteFolderPath));
                Console.WriteLine("无法正常解析表格容器模板");
                return;
            }
            else if (!File.Exists(TempalteFolderPath + mExcelContainerTemplateFileName))
            {
                Console.WriteLine(string.Format("模板文件不存在 : {0}{1}", TempalteFolderPath, mExcelContainerTemplateFileName));
                Console.WriteLine("无法正常解析表格容器模板");
                return;
            }
            else
            {
                var templatefilefullpath = TempalteFolderPath + mGameDataManagerTemplateFileName;
                var templatecontent = File.ReadAllText(templatefilefullpath);

                // 遍历所有excel生成GameDataContainer.cs
                // 重置模板类容
                mTemplateInstance.resetContent(templatecontent);

                // 循环替换数据加载成员定义里里的类名
                mTemplateInstance.beginLoop("#CONTAINER_MEMBER_LOOP#");
                foreach (var excelinfo in mExcelInfoList)
                {
                    // 替换数据加载成员定义里的类名
                    mTemplateInstance.setValue("#CLASS_NAME#", excelinfo.ExcelName);
                    mTemplateInstance.nextLoop();
                }
                mTemplateInstance.endLoop();

                // 循环替换数据加载里的类名
                mTemplateInstance.beginLoop("#CONTAINER_LOAD_LOOP#");
                foreach (var excelinfo in mExcelInfoList)
                {
                    // 替换数据加载循环里的类名
                    mTemplateInstance.setValue("#LOOP_CLASS_NAME#", excelinfo.ExcelName);
                    mTemplateInstance.nextLoop();
                }
                mTemplateInstance.endLoop();

                // 输出生成内容到文件
                var outputfilefullpath = TemplateCSOutputPath + "GameDataManager.cs";
                File.WriteAllText(outputfilefullpath, mTemplateInstance.getContent());

                mTemplateInstance.resetContent(string.Empty);
            }
        }
    }
}
