/*
 * Description:             DataAccess.cs
 * Author:                  TONYTANG
 * Create Date:             2018/12/31
 */

using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DataAccess.cs
/// 逻辑层数据访问统一入口
/// </summary>
public static class DataAccess
{

    /// <summary>
    /// 读取指定id的全局表数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static t_global readGlobalData(int id)
    {
        var globalcontainer = GameDataManager.Singleton.t_globalcontainer.getMap();
        if (globalcontainer.ContainsKey(id))
        {
            return globalcontainer[id];
        }
        else
        {
            Debug.LogError(string.Format("找不到全局表ID : {0}数据配置！", id));
            return null;
        }
    }

    /// <summary>
    /// 读取指定id的作者信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static t_author_Info readAutorInfo(int id)
    {
        var authorcontainer = GameDataManager.Singleton.t_author_Infocontainer.getMap();
        if (authorcontainer.ContainsKey(id))
        {
            return authorcontainer[id];
        }
        else
        {
            Debug.LogError(string.Format("找不到作者信息表ID : {0}数据配置！", id));
            return null;
        }
    }

    /// <summary>
    /// 读取指定id的语言包字符串信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string readLanguageInfo(int id)
    {
        var languagecontainer = GameDataManager.Singleton.t_languagecontainer.getMap();
        if (languagecontainer.ContainsKey(id))
        {
            return languagecontainer[id].content;
        }
        else
        {
            Debug.LogError(string.Format("找不到语言包表ID : {0}数据配置！", id));
            return null;
        }
    }

    /// <summary>
    /// 读取指定id的语言包信息
    /// </summary>
    /// <param name="id"></param>
    /// <param paras="">参数</param>
    /// <returns></returns>
    public static string readLanguageInfo(int id, params object[] paras)
    {
        var languagecontainer = GameDataManager.Singleton.t_languagecontainer.getMap();
        if (languagecontainer.ContainsKey(id))
        {
            return string.Format(languagecontainer[id].content, paras);
        }
        else
        {
            Debug.LogError(string.Format("找不到语言包表ID : {0}数据配置！", id));
            return null;
        }
    }
}