/*
 * Description:             AtlasManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using TUI;
using UnityEngine;
using UnityEngine.U2D;
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
    public AtlasManager()
    {
        DIYLog.Log("添加SpriteAtals图集延时绑定回调!");
        SpriteAtlasManager.atlasRequested += onAtlasRequested;
    }

    /// <summary>
    /// 响应SpriteAtlas图集加载回调
    /// </summary>
    /// <param name="atlasname"></param>
    /// <param name="callback"></param>
    private void onAtlasRequested(string atlasname, Action<SpriteAtlas> callback)
    {
        DIYLog.Log($"加载SpriteAtlas:{atlasname}");
        // Later Bind -- 依赖使用SpriteAtlas的加载都会触发这里
        ResourceModuleManager.Singleton.requstResource(
        atlasname,
        (abi) =>
        {
            DIYLog.Log($"Later Bind加载SpriteAtlas:{atlasname}");
            var sa = abi.loadAsset<SpriteAtlas>(atlasname);
            callback(sa);
        });
    }

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

    /// <summary>
    /// 设置Image指定图片
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlasname">图集名</param>
    /// <param name="spritename">图片名</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setImageSprite(TImage timg, string atlasname, string spritename, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "setImageSprite不允许传空TImage!");
        ResourceModuleManager.Singleton.requstResource(atlasname,
        (abi) =>
        {
            // 清除老的资源引用
            if (timg.ABI != null)
            {
                timg.ABI.releaseOwner(timg);
            }
            if (abi != null)
            {
                var sprite = abi.getAsset<Sprite>(timg, spritename);
                timg.sprite = sprite;
            }
            timg.ABI = abi;
            timg.AtlasName = atlasname;
            timg.SpriteName = spritename;
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置Image指定图片(从Sprite Atlas里)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlasname">图集名</param>
    /// <param name="spritename">图片名</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setImageSpriteAtlas(TImage timg, string atlasname, string spritename, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        ResourceModuleManager.Singleton.requstResource(atlasname,
        (abi) =>
        {
            DIYLog.Log("加载SpriteAtlas AB完成!");
            // 清除老的资源引用
            if (timg.ABI != null)
            {
                timg.ABI.releaseOwner(timg);
            }
            if (abi != null)
            {
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = abi.getAsset<SpriteAtlas>(timg, atlasname);
                DIYLog.Log("加载SpriteAtlas之后!");
                timg.sprite = spriteatlas.GetSprite(spritename);
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
            }
            timg.ABI = abi;
            timg.AtlasName = atlasname;
            timg.SpriteName = spritename;
        },
        loadtype,
        loadmethod);
    }
}