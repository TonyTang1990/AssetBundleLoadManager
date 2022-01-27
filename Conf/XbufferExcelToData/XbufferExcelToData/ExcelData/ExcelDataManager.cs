/*
 * Description:             表格数据读取类
 * Author:                  tanghuan
 * Create Date:             2018/09/02
 */

using Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    /// <summary>
    /// 表格数据单例读取管理类
    /// </summary>
    public class ExcelDataManager : SingletonTemplate<ExcelDataManager>
    {
        /// <summary> Excel目录路径 /// </summary>
        public string ExcelFolderPath { get; private set; }

        /// <summary> 所有的excel文件 /// </summary>
        public List<string> AllExcelFilesList { get; private set; }

        /// <summary>
        /// 表格信息映射Map
        /// Key为表格名字，Value为对应表格的信息
        /// </summary>
        public Dictionary<string, ExcelInfo> ExcelsInfoMap { get; private set; }

        #region Excel数据规则
        /// <summary>
        /// 不参与导表的sheet名开头标识
        /// </summary>
        private const string BLACK_LIST_PREFIX = "blacklist";

        /// <summary>
        /// 有效的Id类型列表
        /// </summary>
        public static List<string> ValideIdTypeList = new List<string>
        {
            "int",
            "string"
        };

        /// <summary>
        /// 第一列字段名(第一列字段名必须是Id)
        /// </summary>
        public const string ID_NAME = "Id";

        /// <summary>
        /// 字段名字行号
        /// </summary>
        private const int FieldNameLineNumber = 1;

        /// <summary>
        /// 字段注释行号
        /// </summary>
        private const int FieldNotationLineNumber = 2;

        /// <summary>
        /// 数据类型行号
        /// </summary>
        private const int FieldTypeLineNumber = 3;

        /// <summary>
        /// 分割信息(仅用于一维和多维数组数据)行号
        /// </summary>
        private const int FieldSpliterLineNumber = 4;

        /// <summary>
        /// 占位符1行号
        /// </summary>
        private const int FieldPlaceHolder1LineNumber = 5;

        /// <summary>
        /// 占位符2行号
        /// </summary>
        private const int FieldPlaceHolder2LineNumber = 6;

        /// <summary>
        /// 数据开始行号
        /// </summary>
        private const int DataLineNumber = 7;
        #endregion

        /// <summary>
        /// Excel文件有效的后缀名过滤
        /// </summary>
        private List<string> ValideExcelPostFixFilter;

        /// <summary>
        /// 有效的数据类型配置
        /// </summary>
        private List<string> ValideTypesList;

        /// <summary>
        /// 有效的分割符号配置
        /// </summary>
        private List<char> ValideSplitersList;

        /// <summary>
        /// 有效的类型与Xbuffer支持的类型映射Map(用于快速访问表格配置的对应Xbuffer的类型字符串)
        /// Key为有效的类型名，Value为对应Xbuffer支持的对应类型名
        /// </summary>
        private Dictionary<string, string> mValideTypesXbufferTypeMap;

        /// <summary>
        /// 临时id判定Map(优化GC问题)
        /// </summary>
        private Dictionary<string, string> mTempIdMap;

        public ExcelDataManager()
        {
            ExcelFolderPath = string.Empty;
            AllExcelFilesList = new List<string>();
            ExcelsInfoMap = new Dictionary<string, ExcelInfo>();
            ValideExcelPostFixFilter = new List<string>(new string[] { "*.xlsx", "*.xls", "*.csv" });
            ValideTypesList = new List<string>(new string[] 
                                                { "notation", // 第一个notation是特殊类型，用于表示注释列
                                                  "int", "float", "string", "long", "bool", 
                                                  "int[]", "float[]", "string[]", "long[]", "bool[]"});
            // 为了避免使用过于常见的分隔符，这里定死了支持的分隔符配置
            ValideSplitersList = new List<char>(new char[] { '+', ';', ',', '|'});      
            mValideTypesXbufferTypeMap = new Dictionary<string, string>();
            mValideTypesXbufferTypeMap.Add("notation", "notation");
            mValideTypesXbufferTypeMap.Add("int", "int");
            mValideTypesXbufferTypeMap.Add("float", "float");
            mValideTypesXbufferTypeMap.Add("string", "string");
            mValideTypesXbufferTypeMap.Add("long", "long");
            mValideTypesXbufferTypeMap.Add("bool", "bool");
            mValideTypesXbufferTypeMap.Add("int[]", "[int]");
            mValideTypesXbufferTypeMap.Add("float[]", "[float]");
            mValideTypesXbufferTypeMap.Add("string[]", "[string]");
            mValideTypesXbufferTypeMap.Add("long[]", "[long]");
            mValideTypesXbufferTypeMap.Add("bool[]", "[bool]");

            mTempIdMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// 配置Excel目录路径
        /// </summary>
        /// <param name="excelfolderpath"></param>
        public void configExcelFolderPath(string excelfolderpath)
        {
            ExcelFolderPath = excelfolderpath;
        }

        /// <summary>
        /// 获取对应Xbuffer里支持配置的类型字符串
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public string getFinalXbufferTypeName(string typename)
        {
            if(mValideTypesXbufferTypeMap.ContainsKey(typename))
            {
                return mValideTypesXbufferTypeMap[typename];
            }
            else
            {
                Console.WriteLine(string.Format("找不到类型名 : {0}在Xbuffer里对应的类型名!", typename));
                return string.Empty;
            }
        }

        /// <summary>
        /// 加载所有的Excel数据信息
        /// </summary>
        /// <returns></returns>
        public bool loadAllDataFromExcelFile()
        {
            if(!readAllExcelFiles())
            {
                return false;
            }

            if(!readAllExcelFilesInfo())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 是否是注释类型
        /// </summary>
        /// <param name="typename">类型名字</param>
        /// <returns></returns>
        public bool isNotationType(string typename)
        {
            return ValideTypesList[0].Equals(typename);
        }

        /// <summary>
        /// 读取所有Excel文件
        /// </summary>
        private bool readAllExcelFiles()
        {
            if (Directory.Exists(ExcelFolderPath))
            {
                AllExcelFilesList.Clear();
                foreach (var postfixfilter in ValideExcelPostFixFilter)
                {
                    var files = Directory.GetFiles(ExcelFolderPath, postfixfilter);
                    foreach(var file in files)
                    {
                        if(AllExcelFilesList.Contains(file))
                        {
                            continue;
                        }
                        else
                        {
                            AllExcelFilesList.Add(file);
                        }
                    }
                }
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("Excel目录不存在:{0}", ExcelFolderPath));
                return false;
            }
        }

        /// <summary>
        /// 读取所有Excel文件内部信息
        /// </summary>
        private bool readAllExcelFilesInfo()
        {
            ExcelsInfoMap.Clear();
            bool issuccess = true;
            foreach (var excelfile in AllExcelFilesList)
            {
                // 如果配置有问题，直接退出
                if (issuccess == false)
                {
                    Console.WriteLine("配置有问题，请先修正配置后再导出!");
                    break;
                }
                FileStream fs = File.Open(excelfile, FileMode.Open, FileAccess.Read);
                IExcelDataReader excelreader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                if (!excelreader.IsValid)
                {
                    Console.WriteLine(string.Format("Excel文件:{0}读取失败！", excelfile));
                    issuccess = false;
                    break;
                }
                else
                {
                    #if DEBUG
                    Console.WriteLine(string.Format("Excel文件.Name:{0}", excelreader.Name));
                    #endif
                    var dataset = excelreader.AsDataSet();
                    for (int index = 0, length = dataset.Tables.Count; index < length; index++)
                    {
                        if (isValideSheet(excelreader.Name))
                        {
                            if (hasSheetNameExist(excelreader.Name))
                            {
                                Console.WriteLine(string.Format("Excel:{0}有重名Sheet:{1}!", excelfile, excelreader.Name));
                                issuccess = false;
                                break;
                            }
                            else
                            {
                                var excelinfo = new ExcelInfo();
                                excelinfo.ExcelName = excelreader.Name;
                                int currentlinenumber = 1;
                                while (excelreader.Read())
                                {
                                    //读取每一行的数据
                                    string[] datas = new string[excelreader.FieldCount];
                                    for (int i = 0; i < excelreader.FieldCount; i++)
                                    {
                                        datas[i] = excelreader.GetString(i);
                                    }

                                    // 字段信息行
                                    if (currentlinenumber == FieldNameLineNumber)
                                    {
                                        excelinfo.FieldNames = datas;
                                    }
                                    // 字段注释信息行
                                    else if (currentlinenumber == FieldNotationLineNumber)
                                    {
                                        excelinfo.FieldNotations = datas;
                                    }
                                    // 字段类型信息行
                                    else if (currentlinenumber == FieldTypeLineNumber)
                                    {
                                        excelinfo.FieldTypes = datas;
                                    }
                                    // 字段分隔符信息行
                                    else if (currentlinenumber == FieldSpliterLineNumber)
                                    {
                                        excelinfo.FieldSpliters = datas;
                                    }
                                    // 字段占位符1信息行
                                    else if (currentlinenumber == FieldPlaceHolder1LineNumber)
                                    {
                                        excelinfo.FieldPlaceholder1s = datas;
                                    }
                                    // 字段占位符2信息行
                                    else if (currentlinenumber == FieldPlaceHolder2LineNumber)
                                    {
                                        excelinfo.FieldPlaceholder2s = datas;
                                    }
                                    else if (currentlinenumber >= DataLineNumber)
                                    {
                                        // 存储数据之前，检查一次各字段名字，字段信息等配置是否正确
                                        if (currentlinenumber == DataLineNumber)
                                        {
                                            if (hasInvalideName(excelinfo.FieldNames, excelinfo.FieldTypes))
                                            {
                                                Console.WriteLine(string.Format("Excel Table:{0}", excelreader.Name));
                                                issuccess = false;
                                                break;
                                            }
                                            if (hasInvalideType(excelinfo.FieldTypes))
                                            {
                                                Console.WriteLine(string.Format("Excel Table:{0}", excelreader.Name));
                                                issuccess = false;
                                                break;
                                            }
                                            if (hasInvalideSpliter(excelinfo.FieldSpliters))
                                            {
                                                Console.WriteLine(string.Format("Excel Table:{0}", excelreader.Name));
                                                issuccess = false;
                                                break;
                                            }
                                        }

                                        // 记录每一行所有数据的字段名，字段类型，字段数据
                                        ExcelData[] exceldatas = new ExcelData[datas.Length];
                                        for (int m = 0; m < datas.Length; m++)
                                        {
                                            ExcelData cd = new ExcelData();
                                            cd.Type = excelinfo.FieldTypes[m];
                                            cd.Name = excelinfo.FieldNames[m];
                                            cd.Spliter = excelinfo.FieldSpliters[m];
                                            cd.Data = datas[m];
                                            exceldatas[m] = cd;
                                        }

                                        if (issuccess == false)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            excelinfo.addData(exceldatas);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine(string.Format("无效的行号:{0}", currentlinenumber));
                                        issuccess = false;
                                        break;
                                    }
                                    currentlinenumber++;
                                }

                                // 检查是否有重复的id
                                if (!isIdValide(excelinfo))
                                {
                                    Console.WriteLine($"Excel Sheet:{excelinfo.ExcelName}的配置有问题!");
                                    issuccess = false;
                                    break;
                                }

                                ExcelsInfoMap.Add(excelreader.Name, excelinfo);
                            }
                        }
                        excelreader.NextResult();
                    }
                }
            }

            if(!issuccess)
            {
                clearExcelInfo();
                return false;
            }
            else
            {
#if DEBUG
                //打印表格数据信息
                //foreach(var excelinfo in ExcelsInfoMap)
                //{
                //    excelinfo.Value.printOutAllExcelInfo();
                //}
#endif
                return true;
            }
        }

        /// <summary>
        /// 是否是有效Sheet
        /// </summary>
        /// <param name="sheetname"></param>
        /// <returns></returns>
        private bool isValideSheet(string sheetname)
        {
            return !sheetname.StartsWith(BLACK_LIST_PREFIX);
        }

        /// <summary>
        /// 指定sheet名是否存在(不区分大小写)
        /// </summary>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private bool hasSheetNameExist(string sheetName)
        {
            foreach(var excelInfo in ExcelsInfoMap)
            {
                if(excelInfo.Value.ExcelName.Equals(sheetName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否有重复的字段名
        /// 字段名不能是否有为空的(不含注释类型)
        /// 注释类型不允许填字段名
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        private bool hasInvalideName(string[] names, string[] types)
        {
            var length = names != null ? names.Length : 0;
            if(length == 0)
            {
                Console.WriteLine($"未配置有效字段和类型信息!");
                return true;
            }
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    else
                    {
                        // 注释类型强制不允许配置字段名，注释类型不需要名字检查
                        bool isnotationtype = ExcelDataManager.Singleton.isNotationType(types[i]);
                        if(isnotationtype)
                        {
                            if (!string.IsNullOrEmpty(names[i]))
                            {
                                Console.WriteLine(string.Format("配置错误 : 注释类型不允许填字段名，字段索引 : {0}!", i));
                                return true;
                            }
                            else
                            {
                                // 注释类型不参与后面的检查
                                continue;
                            }
                        }
                        else if (string.IsNullOrEmpty(names[i]))
                        {
                            Console.WriteLine(string.Format("配置错误 : 字段名不能为空，字段索引 : {0}!", i));
                            return true;
                        }
                        else if (names[i].Equals(names[j]))
                        {
                            Console.WriteLine(string.Format("配置错误 : 同名字段:{0}!", names[i]));
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否有无效的类型配置且第一列必须是int类型(强制限制第一类数据类型，第一列用于填id)
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private bool hasInvalideType(string[] types)
        {
            for(int i = 0, length = types.Length; i < length; i++)
            {
                if(i == 0 && !isValideIdType(types[i]))
                {
                    Console.WriteLine($"配置错误 : 第一列的数据类型:{types[i]}不支持");
                    return true;
                }
                else
                {
                    if (!ValideTypesList.Contains(types[i]))
                    {
                        Console.WriteLine(string.Format("配置错误 : 不支持的数据类型配置:{0}", types[i]));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否有不被支持的分隔符配置
        /// </summary>
        /// <param name="spliters"></param>
        /// <returns></returns>
        private bool hasInvalideSpliter(string[] spliters)
        {
            foreach (var spliter in spliters)
            {
                if (spliter != null)
                {
                    var allspliters = spliter.ToCharArray();
                    // 只支持一个分隔符即一维数组配置
                    if (allspliters.Length != 1)
                    {
                        Console.WriteLine("只支持最多一个的分隔符配置!");
                        return true;
                    }
                    else
                    {
                        foreach (var sp in allspliters)
                        {
                            if (!ValideSplitersList.Contains(sp))
                            {
                                Console.WriteLine(string.Format("配置错误 : 不支持的分隔符配置:{0}", sp));
                                Console.WriteLine(string.Format("支持的有效分隔符如下: {0}", ValideSplitersList.ToString()));
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// Id是否有效
        /// </summary>
        private bool isIdValide(ExcelInfo excelInfo)
        {
            var idType = excelInfo.getIdType();
            if (!isValideIdType(idType))
            {
                Console.WriteLine($"id类型:{idType}不支持,仅支持int和string!");
                return false;
            }
            if(hasDuplicatedId(excelInfo))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 指定的类型是否是有效id类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool isValideIdType(string type)
        {
            return ValideIdTypeList.Contains(type);
        }
    
        /// <summary>
        /// 是否有重复的id
        /// </summary>
        /// <param name="excelinfo"></param>
        /// <returns></returns>
        private bool hasDuplicatedId(ExcelInfo excelinfo)
        {
            if(excelinfo != null)
            {
                mTempIdMap.Clear();
                foreach(var data in excelinfo.DatasList)
                {
                    if(mTempIdMap.ContainsKey(data[0].Data))
                    {
                        Console.WriteLine(string.Format("重复的id : {0}", data[0].Data));
                        return true;
                    }
                    else
                    {
                        mTempIdMap.Add(data[0].Data, data[0].Data);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 清除Excel信息
        /// </summary>
        private void clearExcelInfo()
        {
            ExcelsInfoMap.Clear();
        }
    }
}
