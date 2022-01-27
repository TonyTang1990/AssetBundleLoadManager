/*
 * Description:             基于Xbuffer的导表工具
 * Author:                  tanghuan
 * Create Date:             2018/08/28
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 基于Xbuffer的导表工具
/// </summary>
namespace XbufferExcelToData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("控制台参数个数:" + args.Length);
            var argfulltext = string.Empty;
            for(int i = 0, length = args.Length; i < length; i++)
            {
                argfulltext += args[i];
                if (i != length - 1)
                {
                    argfulltext += " ";
                }
            }
            Console.WriteLine("控制台参数:" + argfulltext);
            var disableouputcscode = args.Length > 0 ? (bool.TryParse(args[0], out bool disablecscode) == true ? disablecscode : false) : false;
            if(disableouputcscode)
            {
                Console.WriteLine("关闭生成CS代码!");
            }
            else
            {
                Console.WriteLine("开启生成CS代码!");
            }

            // 读取导出配置信息
            TimeCounter.Singleton.Restart("读取导出配置");
            if(!XbufferExcelExportConfig.Singleton.LoadExportConfigData())
            {
                Console.WriteLine("读取导出配置文件信息失败！");
                Console.ReadKey();
                return;
            }
            TimeCounter.Singleton.End();

            // 设置表格路径以及读取所有excel数据
            TimeCounter.Singleton.Restart("读取所有excel数据");
            ExcelDataManager.Singleton.configExcelFolderPath(XbufferExcelExportConfig.Singleton.ExportConfigInfo.ExcelInputPath);
            if(!ExcelDataManager.Singleton.loadAllDataFromExcelFile())
            {
                Console.WriteLine("读取表格数据失败！");
                Console.ReadKey();
                return;
            }
            TimeCounter.Singleton.End();

            // 自动化生成Excel对应的描述文件
            TimeCounter.Singleton.Restart("生成Excel对应描述文件");
            XbufferExcelToDesFile.Singleton.configDesFileOutputFolderPath(XbufferExcelExportConfig.Singleton.ExportConfigInfo.DesFileOutputPath);
            Utilities.RecreateSpecificFolder(XbufferExcelToDesFile.Singleton.DesFileFolderPath);
            var excelinfomap = ExcelDataManager.Singleton.ExcelsInfoMap;
            foreach(var excelinfo in excelinfomap)
            {
                var exceldata = excelinfo.Value;
                if(!XbufferExcelToDesFile.Singleton.writeExcelDataToDesFile(exceldata.ExcelName, exceldata.FieldNames, exceldata.FieldTypes, exceldata.FieldNotations))
                {
                    Console.WriteLine(string.Format("写入Excel : {0}的数据结构文件失败!", exceldata.ExcelName));
                    Console.ReadKey();
                    return;
                }
            }
            TimeCounter.Singleton.End();

            // 自动化生成序列化所需的代码文件
            if(disableouputcscode == false)
            {
                TimeCounter.Singleton.Restart("生成序列化代码文件");
                XbufferDesFileToCSCode.Singleton.configFolderPath(XbufferExcelExportConfig.Singleton.ExportConfigInfo.DesFileOutputPath,
                                                                  XbufferExcelExportConfig.Singleton.ExportConfigInfo.TemplatePath,
                                                                  XbufferExcelExportConfig.Singleton.ExportConfigInfo.CSClassCodeOutputPath,
                                                                  XbufferExcelExportConfig.Singleton.ExportConfigInfo.CSBufferCodeOutputPath);
                XbufferDesFileToCSCode.Singleton.writeAllDesFileToCSCode();
                TimeCounter.Singleton.End();
            }

            // 序列化数据
            TimeCounter.Singleton.Restart("序列化数据");
            XbufferExcelDataToBytes.Singleton.configBytesOutputFolderPath(XbufferExcelExportConfig.Singleton.ExportConfigInfo.ByteDataOutputPath);
            Utilities.RecreateSpecificFolder(XbufferExcelDataToBytes.Singleton.BytesFolderPath);
            foreach (var excelinfo in ExcelDataManager.Singleton.ExcelsInfoMap)
            {
                if(!XbufferExcelDataToBytes.Singleton.writeExcelDataToBytes(excelinfo.Value))
                {
                    Console.WriteLine("序列化数据出问题!请检查表格配置!");
                    Console.ReadKey();
                    return;
                }
            }
            TimeCounter.Singleton.End();

            // 自动化生成表格加载相关代码
            if (disableouputcscode == false)
            {
                TimeCounter.Singleton.Restart("生成表格加载代码");
                XbufferTemplateToCSCode.Singleton.configTemplateInfo(XbufferExcelExportConfig.Singleton.ExportConfigInfo.TemplatePath,
                                                                     XbufferExcelExportConfig.Singleton.ExportConfigInfo.CSTemplateOutputPath,
                                                                     ExcelDataManager.Singleton.ExcelsInfoMap.Values.ToList<ExcelInfo>());
                Utilities.RecreateSpecificFolder(XbufferTemplateToCSCode.Singleton.TemplateCSOutputPath);
                XbufferTemplateToCSCode.Singleton.parseGameDataManagerTemplate();
                XbufferTemplateToCSCode.Singleton.parseExcelContainerTemplate();
                TimeCounter.Singleton.End();
            }

            Console.WriteLine("导表完成!输入任意键结束!");
            Console.ReadKey();
        }
    }
}
