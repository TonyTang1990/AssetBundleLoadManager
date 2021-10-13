/*
 * Description:             ResourceHelper.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/12
 */

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// ResourceHelper.cs
/// 资源辅助类
/// </summary>
public static class ResourceHelper
{
    /// <summary>
    /// 有效Asset后缀Map<后缀名,是否有效>
    /// </summary>
    private static Dictionary<string, bool> mValideAssetPostFixMap = new Dictionary<string, bool>()
    {
        { ".prefab", true },
        { ".fbx", true },
        { ".mat", true },
        { ".png", true },
        { ".PNG", true },
        { ".jpg", true },
        { ".JPG", true },
        { ".mp3", true },
        { ".wav", true },
        { ".ogg", true },
        { ".shader", true },
        { ".anim", true },
        { ".spriteatlas", true },
        { ".playable", true },
        { ".asset", true },
    };

    /// <summary>
    /// 指定Asset路径是否有效后缀
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public static bool IsAssetPathHasValideAssetPostfix(string assetPath)
    {
        string ext = Path.GetExtension(assetPath);
        bool result = false;
        mValideAssetPostFixMap.TryGetValue(ext, out result);
        return result;
    }
}