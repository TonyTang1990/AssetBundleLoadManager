/*
 * Description:             GameConfigModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2023/02/10
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// GameConfigModuleManager.cs
/// 游戏配置信息管理模块单例类
/// </summary>
public class GameConfigModuleManager : SingletonTemplate<GameConfigModuleManager>
{
    /// <summary>
    /// 游戏配置信息文件名
    /// </summary>
    private const string GameConfigFileName = "GameConfig";

    /// <summary>
    /// 游戏配置信息文件存储路径(不含文件后缀)
    /// </summary>
    public string GameConfigFilePath
    {
        get;
        private set;
    }

#if UNITY_EDITOR
    /// <summary> 游戏配置信息文件存储全路径 /// </summary>
    public string GameConfigSaveFileFullPath
    {
        get;
        private set;
    }
#endif

    /// <summary>
    /// 包内游戏配置信息
    /// </summary>
    public GameConfig InnerGameConfig
    {
        get;
        private set;
    }

    /// <summary>
    /// UTF8编码
    /// </summary>
    private UTF8Encoding mUTF8Encoding = new UTF8Encoding(true);

    public GameConfigModuleManager()
    {
        GameConfigFilePath = $"Config/{GameConfigFileName}";
#if UNITY_EDITOR
        GameConfigSaveFileFullPath = $"{Application.dataPath}/Resources/{GameConfigFilePath}.json";
#endif
        InnerGameConfig = null;
    }

    /// <summary>
    /// 初始化游戏配置信息
    /// </summary>
    /// <returns></returns>
    public void initGameConfigData()
    {
        InnerGameConfig = null;
        Debug.Log($"游戏配置信息文件:{GameConfigFilePath}!");
        var gameConfigAsset = Resources.Load<TextAsset>(GameConfigFilePath);
        if (gameConfigAsset != null)
        {
            Debug.Log("游戏配置信息:");
            var content = mUTF8Encoding.GetString(gameConfigAsset.bytes);
            Debug.Log($"content:{content}");
            InnerGameConfig = JsonUtility.FromJson<GameConfig>(content);
            Debug.Log($"DevelopMode:{InnerGameConfig.DevelopMode}");
            if(InnerGameConfig.DevelopMode == GameDevelopMode.Invalide)
            {
                Debug.LogError($"配置了无效的游戏开发模式:{InnerGameConfig.DevelopMode}，请检查配置!");
            }
        }
        else
        {
            Debug.LogError($"严重错误！找不到游戏配置信息文件:{GameConfigFilePath}!无法读取!");
        }
        InnerGameConfig = InnerGameConfig == null ? new GameConfig() : InnerGameConfig;
    }

    /// <summary>
    /// 获取当前配置游戏开发模式
    /// </summary>
    /// <returns></returns>
    public GameDevelopMode GetGameDevelopMode()
    {
        return InnerGameConfig != null ? InnerGameConfig.DevelopMode : GameDevelopMode.Invalide;
    }

    /// <summary>
    /// 是否处于内网开发模式
    /// </summary>
    /// <returns></returns>
    public bool IsInnerDevelopMode()
    {
        return InnerGameConfig != null ? InnerGameConfig.DevelopMode == GameDevelopMode.InnerDevelop : false;
    }

    /// <summary>
    /// 是否处于发布模式
    /// </summary>
    /// <returns></returns>
    public bool IsReleaseMode()
    {
        return InnerGameConfig != null ? InnerGameConfig.DevelopMode == GameDevelopMode.Release : false;
    }

    #region 限Editor使用
#if UNITY_EDITOR
    /// <summary>
    /// 存储最新游戏开发模式到配置信息
    /// </summary>
    /// <param name="developModel">游戏开发模式</param>
    public void saveGameDevelopModel(GameDevelopMode developModel)
    {
        Debug.Log($"存储最新游戏开发模式:{developModel}到包内!");
        Debug.Log($"GameConfigSaveFileFullPath:{GameConfigSaveFileFullPath}");

        if (InnerGameConfig == null)
        {
            Debug.LogError("找不到包内游戏配置信息!无法存储新的游戏开发模式配置信息!");
            return;
        }

        InnerGameConfig.DevelopMode = developModel;
        Debug.Log($"developModel:{developModel}");

        var gameConfigdata = JsonUtility.ToJson(InnerGameConfig);
        using (var vgameConfigFs = File.Open(GameConfigSaveFileFullPath, FileMode.Create))
        {
            byte[] gameConfigInfo = mUTF8Encoding.GetBytes(gameConfigdata);
            vgameConfigFs.Write(gameConfigInfo, 0, gameConfigInfo.Length);
            vgameConfigFs.Close();
        }
    }
#endif
    #endregion
}