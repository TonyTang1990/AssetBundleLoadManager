/*
 * Description:             全局常量定义静态类
 * Author:                  tanghuan
 * Create Date:             2018/09/02
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    /// <summary>
    /// 全局常量定义静态类
    /// </summary>
    public static class ConstValue
    {
        /// <summary> 数据结构定义文件后缀名 /// </summary>
        public const string DesFilePostFix = ".xb";

        /// <summary> CS类模板文件名 /// </summary>
        public const string CSClassTemplateFileName = "csharp_class.ftl";

        /// <summary> CS序列化模板文件名 /// </summary>
        public const string CSBufferTemplateFileName = "csharp_buffer.ftl";

        /// <summary> Xbuffer解析Exe文件路径 /// </summary>
        public const string XbufferParserExePath = "xbuffer_parser.exe";

        /// <summary> Excel二进制数据文件后缀 /// </summary>
        public const string ExcelBytesDataFilePostFix = ".bytes";
    }
}
