/*
 * Description:             导表工具导出配置数据抽象
 * Author:                  tanghuan
 * Create Date:             2018/09/01
 */

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    public class ExportConfig
    {
        /// <summary> 表格数据路径 /// </summary>
        public string ExcelInputPath { get; set; }

        /// <summary> Xbuffer模板路径 /// </summary>
        public string TemplatePath { get; set; }

        /// <summary> Excel对应的描述文件生成路径 /// </summary>
        public string DesFileOutputPath { get; set; }

        /// <summary> 序列化数据输出路径 /// </summary>
        public string ByteDataOutputPath { get; set; }

        /// <summary> CS类代码输出路径 /// </summary>
        public string CSClassCodeOutputPath { get; set; }

        /// <summary> CS序列化代码输出路径 /// </summary>
        public string CSBufferCodeOutputPath { get; set; }

        /// <summary> CS模板文件代码输出路径 /// </summary>
        public string CSTemplateOutputPath { get; set; }

        /// <summary> 其他语言代码输出路径 /// </summary>
        public string OtherLanguageCodeOutputPath { get; set; }

        /// <summary>
        /// 打印所有信息
        /// </summary>
        public void printOutAllInfo()
        {
            Console.WriteLine("导出路径配置如下:");
            Console.WriteLine(string.Format("ExcelInputPath:\n{0}", ExcelInputPath));
            Console.WriteLine(string.Format("TemplatePath:\n{0}", TemplatePath));
            Console.WriteLine(string.Format("DesFileOutputPath:\n{0}", DesFileOutputPath));
            Console.WriteLine(string.Format("ByteDataOutputPath:\n{0}", ByteDataOutputPath));
            Console.WriteLine(string.Format("CSClassCodeOutputPath:\n{0}", CSClassCodeOutputPath));
            Console.WriteLine(string.Format("CSBufferCodeOutputPath\n:{0}", CSBufferCodeOutputPath));
            Console.WriteLine(string.Format("CSTemplateOutputPath:\n{0}", CSTemplateOutputPath));
            Console.WriteLine(string.Format("OtherLanguageCodeOutputPath:\n{0}", OtherLanguageCodeOutputPath));
            Console.WriteLine();
        }
    }
}
