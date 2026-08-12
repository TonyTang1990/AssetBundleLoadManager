/*
 * Description:             GameVersionConfigManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/12
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TResource;
using UnityEngine;

/// <summary>
/// VersionConfigModuleManager.cs
/// 游戏版本信息管理模块单例类
/// </summary>
public class VersionConfigModuleManager : SingletonTemplate<VersionConfigModuleManager>
{
    /// <summary>
    /// 包内版本信息文件存储相对Resources路径
    /// </summary>
    public string InnerVersionConfigFileRelativePath
    {
        get;
        private set;
    }

#if UNITY_EDITOR
    /// <summary> 包内资源版本信息文件存储全路径 /// </summary>
    public string InnerVersionConfigSaveFileFullPath
    {
        get;
        private set;
    }
#endif

    /// <summary> 包外资源版本信息文件存储目录路径 /// </summary>
    public string OutterVersionConfigSaveFileFolderPath
    {
        get;
        private set;
    }

    /// <summary> 包外资源版本信息文件存储路径 /// </summary>
    public string OutterVersionConfigSaveFileFullPath
    {
        get;
        private set;
    }

    /// <summary>
    /// 游戏版本信息
    /// </summary>
    public VersionConfig GameVersionConfig
    {
        get;
        private set;
    }

    /// <summary>
    /// 包内版本信息
    /// </summary>
    public VersionConfig InnerGameVersionConfig
    {
        get;
        private set;
    }

    /// <summary>
    /// 包外版本信息
    /// </summary>
    public VersionConfig OuterGameVersionConfig
    {
        get;
        private set;
    }

    /// <summary>
    /// UTF8编码
    /// </summary>
    private UTF8Encoding mUTF8Encoding = new UTF8Encoding(true);

    public VersionConfigModuleManager()
    {
        InnerVersionConfigFileRelativePath = ResourcePath.GetInnerVersionConfigRelativePath();
#if UNITY_EDITOR
        InnerVersionConfigSaveFileFullPath = ResourcePath.GetInnerVersionConfigFullPath();
#endif
        OutterVersionConfigSaveFileFolderPath = ResourcePath.GetOutterVersionConfigFolderPath();
        OutterVersionConfigSaveFileFullPath = ResourcePath.GetOtterVersionConfigFullPath();
        GameVersionConfig = null;
        InnerGameVersionConfig = null;
        OuterGameVersionConfig = null;
    }

    /// <summary>
    /// 存储最新版本号信息到包外
    /// </summary>
    /// <param name="versionCode">版本号</param>
    public void SaveNewVersionCodeOuterConfig(double versionCode)
    {
        //TODO:包外版本信息存储
        Debug.Log($"OutterVersionConfigSaveFileFullPath : {OutterVersionConfigSaveFileFullPath}");

        if (GameVersionConfig == null)
        {
            Debug.LogError("找不到版本信息!无法存储新的版本信息!");
            return;
        }

        if (!Directory.Exists(OutterVersionConfigSaveFileFolderPath))
        {
            Directory.CreateDirectory(OutterVersionConfigSaveFileFolderPath);
        }

        GameVersionConfig.VersionCode = versionCode;
        Debug.Log("versionCode = " + versionCode);

        var versionConfigData = JsonUtility.ToJson(GameVersionConfig);
        using (var verisionConfigFS = new StreamWriter(OutterVersionConfigSaveFileFullPath, false, Encoding.UTF8))
        {
            verisionConfigFS.Write(versionConfigData);
        }
    }

    /// <summary>
    /// 存储最新资源版本号信息到包外
    /// </summary>
    /// <param name="resourceVersionCode">资源版本号</param>
    public void SaveNewResoueceCodeOuterConfig(int resourceVersionCode)
    {
        //TODO:包外版本信息存储
        Debug.Log($"OutterVersionConfigSaveFileFullPath : {OutterVersionConfigSaveFileFullPath}");

        if (GameVersionConfig == null)
        {
            Debug.LogError("找不到包内版本信息!无法存储新的版本信息!");
            return;
        }

        if(!Directory.Exists(OutterVersionConfigSaveFileFolderPath))
        {
            Directory.CreateDirectory(OutterVersionConfigSaveFileFolderPath);
        }

        GameVersionConfig.ResourceVersionCode = resourceVersionCode;
        Debug.Log($"VersionCode:{GameVersionConfig.VersionCode}，ResourceVersionCode:{GameVersionConfig.ResourceVersionCode}");
        
        var versionConfigData = JsonUtility.ToJson(GameVersionConfig, true);
        File.WriteAllText(OutterVersionConfigSaveFileFullPath, versionConfigData, new UTF8Encoding(false));
    }

    #region 限Editor使用
#if UNITY_EDITOR
    /// <summary>
    /// 存储最新版本号信息到包内
    /// </summary>
    /// <param name="versionCode">版本号</param>
    public void SaveNewVersionCodeInnerConfig(double versionCode)
    {
        Debug.Log($"存储最新版本号:{versionCode}到包内!");
        Debug.Log($"InnerVersionConfigSaveFileFullPath : {InnerVersionConfigSaveFileFullPath}");

        if (GameVersionConfig == null)
        {
            Debug.LogError("找不到版本信息!无法存储新的版本信息!");
            return;
        }

        GameVersionConfig.VersionCode = versionCode;
        InnerGameVersionConfig.VersionCode = versionCode;
        Debug.Log("versionCode = " + versionCode);

        var versionConfigData = JsonUtility.ToJson(GameVersionConfig);
        using (var verisionConfigFS = new StreamWriter(InnerVersionConfigSaveFileFullPath, false, Encoding.UTF8))
        {
            verisionConfigFS.Write(versionConfigData);
        }
    }

    /// <summary>
    /// 存储最新资源版本号信息到包外
    /// </summary>
    /// <param name="resourceversioncode">资源版本号</param>
    public void SaveNewResoueceCodeInnerConfig(int resourceversioncode)
    {
        Debug.Log($"存储最新资源版本号:{resourceversioncode}到包内!");
        Debug.Log($"InnerVersionConfigSaveFileFullPath : {InnerVersionConfigSaveFileFullPath}");

        if (GameVersionConfig == null)
        {
            Debug.LogError("找不到包内版本信息!无法存储新的版本信息!");
            return;
        }

        GameVersionConfig.ResourceVersionCode = resourceversioncode;
        InnerGameVersionConfig.ResourceVersionCode = resourceversioncode;
        Debug.Log("resourceversioncode = " + resourceversioncode);

        var versionConfigData = JsonUtility.ToJson(GameVersionConfig);
        using (var verisionConfigFS = new StreamWriter(InnerVersionConfigSaveFileFullPath, false, Encoding.UTF8))
        {
            verisionConfigFS.Write(versionConfigData);
        }
    }
#endif
    #endregion

    /// <summary>
    /// 初始化读取版本信息
    /// </summary>
    /// <returns></returns>
    public void InitVerisonConfigData()
    {
        InnerGameVersionConfig = null;
        OuterGameVersionConfig = null;
        Debug.Log($"OutterVersionConfigSaveFileFullPath : {OutterVersionConfigSaveFileFullPath}");
        Debug.Log($"InnerVersionConfigFileRelativePath : {InnerVersionConfigFileRelativePath}");

        //读取包外版本信息
        if (File.Exists(OutterVersionConfigSaveFileFullPath))
        {
            var outterbytes = File.ReadAllBytes(OutterVersionConfigSaveFileFullPath);
            Debug.Log("包外版本信息:");
            var content = mUTF8Encoding.GetString(outterbytes);
            OuterGameVersionConfig = JsonUtility.FromJson<VersionConfig>(content);
            Debug.Log($"VersionCode : {OuterGameVersionConfig.VersionCode} ResourceVersionCode : {OuterGameVersionConfig.ResourceVersionCode}");
        }
        else
        {
            Debug.Log($"包外游戏配置版本信息文件 : {OutterVersionConfigSaveFileFullPath}不存在!读取包内资源版本信息!");
        }

        //读取包内信息
        Debug.Log($"包内游戏配置版本信息文件 : {InnerVersionConfigFileRelativePath}!");
        //读取包内的版本信息
        var versionconfigasset = Resources.Load<TextAsset>(InnerVersionConfigFileRelativePath);
        if (versionconfigasset != null)
        {
            Debug.Log("包内版本信息:");
            var content = versionconfigasset.text;
            Debug.Log($"content : {content}");
            InnerGameVersionConfig = JsonUtility.FromJson<VersionConfig>(content);
            Debug.Log($"VersionCode : {InnerGameVersionConfig.VersionCode} ResourceVersionCode : {InnerGameVersionConfig.ResourceVersionCode}");
        }
        else
        {
            Debug.LogError($"严重错误！包内游戏配置版本信息文件 : {InnerVersionConfigFileRelativePath}不存在!无法读取!");
        }

        //当前版本信息，如果包内比包外游戏版本号高，以包内为准
        //如果包内小于等于包外游戏版本号，以包外的为准
        GameVersionConfig = GameVersionConfig == null ? new VersionConfig() : GameVersionConfig;
        if (OuterGameVersionConfig != null)
        {
            if(InnerGameVersionConfig.VersionCode > OuterGameVersionConfig.VersionCode)
            {
                GameVersionConfig.VersionCode = InnerGameVersionConfig.VersionCode;
                GameVersionConfig.ResourceVersionCode = InnerGameVersionConfig.ResourceVersionCode;
            }
            else
            {
                GameVersionConfig.VersionCode = OuterGameVersionConfig.VersionCode;
                GameVersionConfig.ResourceVersionCode = OuterGameVersionConfig.ResourceVersionCode;
            }
        }
        else
        {
            GameVersionConfig.VersionCode = InnerGameVersionConfig.VersionCode;
            GameVersionConfig.ResourceVersionCode = InnerGameVersionConfig.ResourceVersionCode;
        }
    }

    /// <summary>
    /// 是否需要版本强更
    /// </summary>
    /// <param name="newVersionCode">新版本号</param>
    /// <returns></returns>
    public bool NeedVersionHotUpdate(double newVersionCode)
    {
        return newVersionCode > GameVersionConfig.VersionCode;
    }

    /// <summary>
    /// 是否需要版本强更
    /// </summary>
    /// <param name="newResourceCode">新资源版本号</param>
    /// <returns></returns>
    public bool NeedResourceHotUpdate(int newResourceCode)
    {
        return newResourceCode > GameVersionConfig.ResourceVersionCode;
    }

    /// <summary>
    /// 是否已经版本强更完成
    /// 判定包内版本号是否大于包外版本号
    /// </summary>
    /// <returns></returns>
    public bool HasVersionHotUpdate()
    {
        if(OuterGameVersionConfig != null && InnerGameVersionConfig.VersionCode > OuterGameVersionConfig.VersionCode)
        {
            Debug.Log($"包内版本号 : {InnerGameVersionConfig.VersionCode} 包外版本号 : {OuterGameVersionConfig.VersionCode}");
            return true;
        }
        else
        {
            return false;
        }
    }
}