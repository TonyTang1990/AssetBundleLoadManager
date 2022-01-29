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
    /// 读取指定Key的t_global_s数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static t_global_s readGlobalSData(string key)
    {
        var globalSContainer = GameDataManager.Singleton.Gett_global_sMap();
        if (globalSContainer.ContainsKey(key))
        {
            return globalSContainer[key];
        }
        else
        {
            Debug.LogError(string.Format("找不到t_global_s:{0}数据配置！", key));
            return null;
        }
    }

    /// <summary>
    /// 读取指定Key的t_global_b数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static t_global_b readGlobalBData(string key)
    {
        var globalBContainer = GameDataManager.Singleton.Gett_global_bMap();
        if (globalBContainer.ContainsKey(key))
        {
            return globalBContainer[key];
        }
        else
        {
            Debug.LogError(string.Format("找不到t_global_b:{0}数据配置！", key));
            return null;
        }
    }


    /// <summary>
    /// 读取指定Key的t_global_i数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static t_global_i readGlobalIData(string key)
    {
        var globalIContainer = GameDataManager.Singleton.Gett_global_iMap();
        if (globalIContainer.ContainsKey(key))
        {
            return globalIContainer[key];
        }
        else
        {
            Debug.LogError(string.Format("找不到t_global_i:{0}数据配置！", key));
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
        var authorContainer = GameDataManager.Singleton.Gett_author_InfoMap();
        if (authorContainer.ContainsKey(id))
        {
            return authorContainer[id];
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
    /// <param name="key"></param>
    /// <returns></returns>
    public static string readLanguageInfo(string key)
    {
        // TODO: 多语言判定读取
        var languageContainer = GameDataManager.Singleton.Gett_language_cnMap();
        if (languageContainer.ContainsKey(key))
        {
            return languageContainer[key].Value;
        }
        else
        {
            Debug.LogError(string.Format("找不到语言包表:{0}数据配置！", key));
            return null;
        }
    }

    /// <summary>
    /// 读取指定id的语言包信息
    /// </summary>
    /// <param name="key"></param>
    /// <param paras="">参数</param>
    /// <returns></returns>
    public static string readLanguageInfo(string key, params object[] paras)
    {
        // TODO: 多语言判定读取
        var languageContainer = GameDataManager.Singleton.Gett_language_cnMap();
        if (languageContainer.ContainsKey(key))
        {
            return string.Format(languageContainer[key].Value, paras);
        }
        else
        {
            Debug.LogError(string.Format("找不到语言包表:{0}数据配置！", key));
            return null;
        }
    }
}