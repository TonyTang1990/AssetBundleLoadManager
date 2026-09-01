/*
 * Description:             AndroidIconSet.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/26
 */

using System;
using UnityEngine;

/// <summary>
/// AndroidIconSet.cs
/// Android平台图标集合
/// </summary>
[Serializable]
public class AndroidIconSet
{
    /// <summary>
    /// 图标集合
    /// Android 7.0/API 24 及更早设备，以及不支持 Adaptive Icon 的启动器
    /// 单层普通方形图标
    /// </summary>
    [Header("图标集合")]
    public Texture2D[] LegacyIcons;

    /// <summary>
    /// 自适应图标背景图标集合
    /// Android 8.0/API 26
    /// 起前景层 + 背景层，由系统裁成圆形、圆角矩形等形状
    /// </summary>
    [Header("自适应图标背景图标集合")]
    public Texture2D[] AdaptiveBackgroundIcons;

    /// <summary>
    /// 自适应图标前景图标集合
    /// Android 8.0/API 26
    /// 起前景层 + 背景层，由系统裁成圆形、圆角矩形等形状
    /// </summary>
    [Header("自适应图标前景图标集合")]
    public Texture2D[] AdaptiveForegroundIcons;

    /// <summary>
    /// 偏好圆形图标集合
    /// Android 7.1/API 25 起，部分启动器偏好圆形图标
    /// 单层圆形适配图标
    /// </summary>
    [Header("偏好圆形图标集合")]
    public Texture2D[] RoundIcons;

    /// <summary>
    /// 内容是否有效
    /// </summary>
    /// <returns></returns>
    public bool IsContentValide()
    {
        // Android要求Legacy Icons 6个，AdaptiveForeground Icons 6个，AdaptiveBackground Icons 6个，Round Icons 6个
        if(!IsValideNumberTextures(LegacyIcons, 6))
        {
            Debug.LogError("Legacy Icons 数量不正确或有空纹理Icon!");
            return false;
        }
        else if(!IsValideNumberTextures(AdaptiveForegroundIcons, 6))
        {
            Debug.LogError("Adaptive Foreground Icons 数量不正确或有空纹理Icon!");
            return false;
        }
        else if(!IsValideNumberTextures(AdaptiveBackgroundIcons, 6))
        {
            Debug.LogError("Adaptive Background Icons 数量不正确或有空纹理Icon!");
            return false;
        }
        else if(!IsValideNumberTextures(RoundIcons, 6))
        {
            Debug.LogError("Round Icons 数量不正确或有空纹理Icon!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 指定纹理数组是否符合指定数量要求且无空纹理
    /// </summary>
    /// <param name="textures"></param>
    /// <param name="requiredNumber"></param>
    /// <returns></returns>
    private bool IsValideNumberTextures(Texture2D[] textures, int requiredNumber)
    {
        if(textures == null || textures.Length != requiredNumber)
        {
            return false;
        }
        for(int i = 0; i < textures.Length; i++)
        {
            if(textures[i] == null)
            {
                Debug.LogError($"纹理数组中第 {i} 个纹理为空, 打包失败!");
                return false;
            }
        }
        return true;
    }
}