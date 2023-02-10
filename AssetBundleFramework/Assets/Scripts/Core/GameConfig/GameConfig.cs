/*
 * Description:             GameConfig.cs
 * Author:                  TONYTANG
 * Create Date:             2023/02/10
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏开发模式
/// </summary>
public enum GameDevelopMode
{
    InnerDevelop = 1,           // 内网开发模式
    Release,                    // 发布模式
    Invalide,                   // 无效模式
}

/// <summary>
/// GameConfig.cs
/// 游戏配置抽象
/// </summary>
[Serializable]
public class GameConfig
{
    /// <summary>
    /// 游戏开发模式
    /// </summary>
    [Header("游戏开发模式")]
    public GameDevelopMode DevelopMode = GameDevelopMode.Release;
}