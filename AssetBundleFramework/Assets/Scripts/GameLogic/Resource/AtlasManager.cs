/*
 * Description:             AtlasManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AtlasManager.cs
/// 图集管理单例类
/// Note:
/// 绑定Image后因为没有挂载相关绑定信息，无法逆推Image资源绑定信息
/// 请勿随意缓存Image，Image缓存会导致切换过的图集资源不满足资源释放条件
/// </summary>
public class AtlasManager : SingletonTemplate<AtlasManager>
{
    /// <summary>
    /// 加载指定图集
    /// </summary>
    /// <param name="atlasname">图集名</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void loadAtlas(string atlasname, Action callback, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(
        atlasname,
        (abi) =>
        {
            abi.loadAllAsset<Sprite>();
            callback?.Invoke();
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置Image指定图片
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="atlasname">图集名</param>
    /// <param name="spritename">图片名</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setImageSprite(Image img, string atlasname, string spritename, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(atlasname,
        (abi) =>
        {
            var sprite = abi.getAsset<Sprite>(img, spritename);
            img.sprite = sprite;
        },
        loadtype,
        loadmethod);
    }
}