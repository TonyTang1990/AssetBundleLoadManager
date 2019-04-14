/*
 * Description:             GameConfigManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/12
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

/// <summary>
/// GameConfigManager.cs
/// 游戏版本信息管理单例类
/// </summary>
public class GameConfigManager : SingletonTemplate<GameConfigManager>, IModuleInterface
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string ModuleName
    {
        get
        {
            return this.GetType().ToString();
        }
    }

    /// <summary>
    /// 游戏版本信息配置文件名
    /// </summary>
    private const string mVersionConfigFileName = "VersionConfig";

    /// <summary>
    /// 配置文件目录路径
    /// </summary>
    private const string ConfigFolderPath = "Config/";

    /// <summary>
    /// 包内版本信息文件存储路径
    /// </summary>
    private string mInnerVersionConfigFilePath = ConfigFolderPath + mVersionConfigFileName;

#if UNITY_EDITOR
    /// <summary> 包外资源版本信息文件存储目录 /// </summary>
    private string OutterVersionConfigSaveFileFullPath = Application.dataPath + "/Resources/" + ConfigFolderPath + mVersionConfigFileName + ".json";
#elif UNITY_STANDALONE
    /// <summary> 包外资源版本信息文件存储目录 /// </summary>
    private string OutterVersionConfigSaveFileFullPath = Application.streamingAssetsPath + "/" + ConfigFolderPath + mVersionConfigFileName + ".json";
#elif UNITY_ANDROID
    /// <summary> 包外资源版本信息文件存储目录 /// </summary>
    private string OutterVersionConfigSaveFileFullPath = Application.streamingAssetsPath + "/" + ConfigFolderPath + mVersionConfigFileName + ".json";
#elif UNITY_IOS
    /// <summary> 包外资源版本信息文件存储目录 /// </summary>
    private string OutterVersionConfigSaveFileFullPath = Application.streamingAssetsPath + "/" + ConfigFolderPath + mVersionConfigFileName + ".json";
#endif

    /// <summary>
    /// 游戏版本信息
    /// </summary>
    public VersionConfig GameVersionConfig
    {
        get;
        private set;
    }

    /// <summary>
    /// UTF8编码
    /// </summary>
    private UTF8Encoding mUTF8Encoding = new UTF8Encoding(true);

    /// <summary>
    /// 存储最新版本信息文件
    /// </summary>
    /// <param name="versioncode">版本号</param>
    /// <param name="resourceversioncode">资源版本号</param>
    public void saveVersionConfig(double versioncode, int resourceversioncode)
    {
        //TODO:包外版本信息存储
        Debug.Log(string.Format("VersionConfigSaveFileFullPath : {0}", OutterVersionConfigSaveFileFullPath));

        if (GameVersionConfig == null)
        {
            Debug.LogError("找不到包内版本信息!无法存储新的版本信息!");
            return;
        }

        GameVersionConfig.VersionCode = versioncode;
        GameVersionConfig.ResourceVersionCode = resourceversioncode;
        Debug.Log("newverisoncode = " + versioncode);
        Debug.Log("newresourceversioncode = " + resourceversioncode);

        var versionconfigdata = JsonUtility.ToJson(GameVersionConfig);
        using (var verisionconfigfs = File.Open(OutterVersionConfigSaveFileFullPath, FileMode.Open))
        {
            byte[] versionconfiginfo = mUTF8Encoding.GetBytes(versionconfigdata);
            verisionconfigfs.Write(versionconfiginfo, 0, versionconfiginfo.Length);
            verisionconfigfs.Close();
        }
    }

    /// <summary>
    /// 读取版本信息
    /// </summary>
    /// <returns></returns>
    public void readVerisonConfigData()
    {
        Debug.Log(string.Format("VersionConfigSaveFileFullPath : {0}", OutterVersionConfigSaveFileFullPath));
        Debug.Log(string.Format("mInnerVersionConfigFilePath : {0}", mInnerVersionConfigFilePath));
        //读取包内的版本信息
        var versionconfigasset = Resources.Load<TextAsset>(mInnerVersionConfigFilePath);
        if (versionconfigasset != null)
        {
            Debug.Log("包内版本信息:");
            var content = mUTF8Encoding.GetString(versionconfigasset.bytes);
            Debug.Log(string.Format("content : {0}", content));
            GameVersionConfig = JsonUtility.FromJson<VersionConfig>(content);
            Debug.Log(string.Format("VersionCode : {0} ResourceVersionCode : {1}", GameVersionConfig.VersionCode, GameVersionConfig.ResourceVersionCode));
        }
        else
        {
            Debug.LogError(string.Format("包内游戏配置版本信息文件 : {0}不存在!无法读取!", mInnerVersionConfigFilePath));
        }
        //读取包外版本信息
        if(File.Exists(OutterVersionConfigSaveFileFullPath))
        {
            var outterbytes = File.ReadAllBytes(OutterVersionConfigSaveFileFullPath);
            Debug.Log("包外版本信息:");
            var content = mUTF8Encoding.GetString(outterbytes);
            Debug.Log(string.Format("content : {0}", content));
            GameVersionConfig = JsonUtility.FromJson<VersionConfig>(content);
            Debug.Log(string.Format("VersionCode : {0} ResourceVersionCode : {1}", GameVersionConfig.VersionCode, GameVersionConfig.ResourceVersionCode));
        }
        else
        {
            Debug.LogError(string.Format("包外游戏配置版本信息文件 : {0}不存在!无法读取!", OutterVersionConfigSaveFileFullPath));
        }
    }
}