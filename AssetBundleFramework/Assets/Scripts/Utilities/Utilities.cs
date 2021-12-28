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
}
