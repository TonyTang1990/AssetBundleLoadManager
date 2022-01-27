/*
 * Description:             表格数据抽象
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
    /// 表格信息抽象
    /// </summary>
    public class ExcelInfo
    {
        /// <summary> 表格名字 /// </summary>
        public string ExcelName { get; set; }

        /// <summary> 字段名字 /// </summary>
        public string[] FieldNames { get; set; }

        /// <summary> 字段注释 /// </summary>
        public string[] FieldNotations { get; set; }

        /// <summary> 数据类型 /// </summary>
        public string[] FieldTypes { get; set; }

        /// <summary> 分割信息(仅用于一维和多维数组数据) /// </summary>
        public string[] FieldSpliters { get; set; }

        /// <summary> 占位符1 /// </summary>
        public string[] FieldPlaceholder1s { get; set; }

        /// <summary> 占位符2 /// </summary>
        public string[] FieldPlaceholder2s { get; set; }

        /// <summary> 表格数据信息列表 /// </summary>
        public List<ExcelData[]> DatasList { get; private set; }

        public ExcelInfo()
        {
            ExcelName = string.Empty;
            FieldNames = null;
            FieldNotations = null;
            FieldTypes = null;
            FieldSpliters = null;
            FieldPlaceholder1s = null;
            FieldPlaceholder2s = null;
            DatasList = new List<ExcelData[]>();
        }

        /// <summary>
        /// 添加数据信息
        /// </summary>
        /// <param name="exceldata"></param>
        public void addData(ExcelData[] exceldata)
        {
            DatasList.Add(exceldata);
        }

        /// <summary>
        /// 获取Id字段名字
        /// </summary>
        /// <returns></returns>
        public string getIdName()
        {
            return FieldNames != null ? FieldNames[0] : string.Empty;
        }

        /// <summary>
        /// 获取Id的类型(仅支持int和string)
        /// </summary>
        /// <returns></returns>
        public string getIdType()
        {
            return FieldTypes != null ? FieldTypes[0] : string.Empty;
        }

        /// <summary>
        /// 打印所有Excel信息
        /// </summary>
        public void printOutAllExcelInfo()
        {
            Console.WriteLine(string.Format("ExcelName : {0}", ExcelName));
            printOutArrayInfo(FieldNames);
            printOutArrayInfo(FieldNotations);
            printOutArrayInfo(FieldTypes);
            printOutArrayInfo(FieldSpliters);

            foreach (var datas in DatasList)
            {
                foreach(var data in datas)
                {
                    data.printOutAllDatas();
                }
                Console.WriteLine();
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
            }
        }

        /// <summary>
        /// 打印数组信息
        /// </summary>
        /// <param name="title">标题分类</param>
        /// <param name="arrayinfo">数据信息</param>
        private void printOutArrayInfo(string[] arrayinfo)
        {
            for(int i = 0, length = arrayinfo.Length; i < length; i++)
            {
                Console.Write(string.Format("{0, -20}", arrayinfo[i]));
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
        }
    }

    /// <summary>
    /// 表格实际数据信息抽象
    /// </summary>
    public class ExcelData
    {
        /// <summary>
        /// 字段名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 分割信息(仅用于一维和多维数组数据)
        /// </summary>
        public string Spliter { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 打印所有数据信息
        /// </summary>
        public void printOutAllDatas()
        {
            Console.Write(string.Format("{0, -20}", Data));
        }
    }
}
