/*
 * Description:             自动化生成Excel数据对应的数据结构定义文件类的单例类
 * Author:                  tanghuan
 * Create Date:             2018/09/02
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    /// <summary>
    /// 自动化生成Excel数据对应的数据结构定义文件类的单例类
    /// </summary>
    public class XbufferExcelToDesFile : SingletonTemplate<XbufferExcelToDesFile>
    {
        /// <summary>
        /// 数据结构定义文件输出目录
        /// </summary>
        public string DesFileFolderPath { get; private set; }

        /// <summary>
        /// UTF8编码
        /// </summary>
        private UTF8Encoding mUTF8Encoding;

        public XbufferExcelToDesFile()
        {
            DesFileFolderPath = string.Empty;
            mUTF8Encoding = new UTF8Encoding(true);
        }

        /// <summary>
        /// 配置数据结构文件输出目录
        /// </summary>
        /// <param name="folderpath"></param>
        public void configDesFileOutputFolderPath(string folderpath)
        {
            DesFileFolderPath = folderpath;
        }

        /// <summary>
        /// 将相关Excel信息写入到对应数据结构描述文件
        /// </summary>
        /// <param name="excelname"></param>
        /// <param name="fieldnames"></param>
        /// <param name="fieldtypes"></param>
        /// <param name="fieldnotations"></param>
        /// <returns></returns>
        public bool writeExcelDataToDesFile(string excelname, string[] fieldnames, string[] fieldtypes, string[] fieldnotations)
        {
            if(string.IsNullOrEmpty(excelname))
            {
                Console.WriteLine("数据结构类名不能为空!");
                return false;
            }
            // Excel数据读取时已经做了配置检查操作了
            /*
            else if(isContainNullOrEmpty(fieldnames))
            {
                Console.WriteLine("字段名不能为空或包含空!");
                return false;
            }
            else if (isContainNullOrEmpty(fieldtypes))
            {
                Console.WriteLine("字段类型不能为空或包含空!");
                return false;
            }
            */
            else if (!((fieldnames.Length == fieldtypes.Length) && (fieldtypes.Length == fieldnotations.Length)))
            {
                Console.WriteLine("字段名和字段类型以及字段注释长度不一致!");
                return false;
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(string.Format("// {0}的注释\n", excelname));     // 因为Xbuffer那方默认强制要填注释，暂时加上
                sb.Append(string.Format("class {0}\n", excelname));
                sb.Append("{\n");
                for(int i = 0, length = fieldnames.Length; i < length; i++)
                {
                    if(ExcelDataManager.Singleton.isNotationType(fieldtypes[i]))
                    {
                        // 注释类型不参与描述文件生成
                        continue;
                    }
                    else
                    {
                        var finalxbuffertypename = ExcelDataManager.Singleton.getFinalXbufferTypeName(fieldtypes[i]);
                        if (string.IsNullOrEmpty(finalxbuffertypename))
                        {
                            return false;
                        }
                        sb.Append(string.Format("{0,-40}{1}\n", "\t" + fieldnames[i] + ":" + finalxbuffertypename + ";\t\t\t", "//" + fieldnotations[i]));
                    }
                }
                sb.Append("}");
                var desfilecontent = sb.ToString();

                var filefullpath = DesFileFolderPath + excelname + ConstValue.DesFilePostFix;
                if(File.Exists(filefullpath))
                {
                    File.Delete(filefullpath);
                }

                using (FileStream fs = File.Create(filefullpath))
                {
                    byte[] contentbytes = mUTF8Encoding.GetBytes(desfilecontent);
                    fs.Write(contentbytes, 0, contentbytes.Length);
                }
                return true;
            }
        }

        /// <summary>
        /// 检查是否包含空或者null
        /// </summary>
        /// <param name="sa"></param>
        /// <returns></returns>
        private bool isContainNullOrEmpty(string[] sa)
        {
            if(sa == null)
            {
                return true;
            }
            else
            {
                foreach(var s in sa)
                {
                    if(string.IsNullOrEmpty(s))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
