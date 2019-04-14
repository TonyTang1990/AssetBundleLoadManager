/*
 * Description:             配置表加载辅助单例类
 * Author:                  tanghuan
 * Create Date:             2018/09/05
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 配置表加载辅助单例类
/// </summary>
public class ConfLoader : SingletonTemplate<ConfLoader>
{

    /// <summary>
    /// Excel表格数据存储目录
    /// </summary>
    public const string ExcelDataFolderPath = "DataBytes/";

    public ConfLoader()
    {

    }

    /// <summary>
    /// 获取表格配置数据的二进制流数据
    /// </summary>
    /// <param name="bytefilename"></param>
    /// <returns></returns>
    public Stream getStreamByteName(string bytefilename)
    {
        var textasset = Resources.Load(ExcelDataFolderPath + bytefilename) as TextAsset;
        var memorystream = new MemoryStream(textasset.bytes);
        return memorystream;
    }
}
