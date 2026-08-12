/*
 * Description:             工具静态类
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;

/// <summary>
/// 工具静态类
/// </summary>
public static class Utilities
{
    /// <summary>
    /// 序列化数据到指定文件
    /// </summary>
    /// <param name="filefullpath"></param>
    /// <param name="obj"></param>
    public static void SerializeDataToFile(string filefullpath, object obj)
    {
        var bf = new BinaryFormatter();
        var s = new FileStream(filefullpath, FileMode.CreateNew, FileAccess.Write);
        bf.Serialize(s, obj);
        s.Close();
    }

    /// <summary>
    /// 反序列化数据到指定对象
    /// </summary>
    /// <param name="filefullpath"></param>
    /// <returns></returns>
    public static System.Object DeserializeDataFromFile(string filefullpath)
    {
        var bf = new BinaryFormatter();
        TextAsset text = Resources.Load<TextAsset>(filefullpath);
        Stream s = new MemoryStream(text.bytes);
        System.Object obj = bf.Deserialize(s);
        s.Close();
        return obj;
    }

    /// <summary>
    /// 组合URL
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="pathParts"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string CombineUrl( string baseUrl, params string[] pathParts)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("基础URL不能为空", nameof(baseUrl));
        }
        var normalizedBaseUrl = baseUrl.TrimEnd('/') + "/";
        var relativePath = string.Join("/", pathParts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part.Trim('/', '\\')));
        return new Uri(new Uri(normalizedBaseUrl), relativePath).AbsoluteUri;
    }
}
