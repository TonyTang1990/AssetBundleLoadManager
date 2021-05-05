/*
 * Description:             AtlasManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.IO;
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
        //DIYLog.Log("添加SpriteAtals图集延时绑定回调!");
        //SpriteAtlasManager.atlasRequested += onAtlasRequested;
    }

    ///// <summary>
    ///// 响应SpriteAtlas图集加载回调
    ///// </summary>
    ///// <param name="atlaspath"></param>
    ///// <param name="callback"></param>
    //private void onAtlasRequested(string atlaspath, Action<SpriteAtlas> callback)
    //{
    //    DIYLog.Log($"加载SpriteAtlas:{atlaspath}");
    //    // Later Bind -- 依赖使用SpriteAtlas的加载都会触发这里
    //    ResourceModuleManager.Singleton.requstResource(
    //    atlaspath,
    //    (abi) =>
    //    {
    //        var assetname = Path.GetFileName(atlaspath);
    //        DIYLog.Log($"Later Bind加载SpriteAtlas:{atlaspath}");
    //        var sa = abi.loadAsset<SpriteAtlas>(assetname);
    //        callback(sa);
    //    });
    //}

    /// <summary>
    /// 加载指定图集
    /// </summary>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void loadAtlas(string atlaspath, Action callback, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(
        atlaspath,
        (abi) =>
        {
            abi.loadAllAsset<Sprite>();
            callback?.Invoke();
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置Image指定图集图片
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="spritename">Sprite名</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setImageAtlasSprite(Image img, string atlaspath, string spritename, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(atlaspath,
        (abi) =>
        {
            spritename = spritename;
            var sprite = abi.getAsset<Sprite>(img, spritename);
            img.sprite = sprite;
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置Image指定图片(单图的时候)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spritepath">Sprite路径</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setImageSingleSprite(Image img, string spritepath, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(img == null, "setImageSingleSprite!");
        ResourceModuleManager.Singleton.requstResource(spritepath,
        (abi) =>
        {
            var spritename = Path.GetFileNameWithoutExtension(spritepath);
            var sprite = abi.getAsset<Sprite>(img, spritename);
            img.sprite = sprite;
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置Image指定图片(从Sprite Atlas里)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="spritename">Sprite名</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setImageSpriteAtlas(TImage timg, string atlaspath, string spritename, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        ResourceModuleManager.Singleton.requstResource(atlaspath,
        (abi) =>
        {
            DIYLog.Log("加载SpriteAtlas AB完成!");
            // 清除老的资源引用
            if (timg.ABI != null && !string.IsNullOrEmpty(timg.AtlasPath))
            {
                timg.ABI.releaseOwner(timg);
            }
            if (abi != null)
            {
                var atlasname = Path.GetFileNameWithoutExtension(atlaspath);
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = abi.getAsset<SpriteAtlas>(timg, atlasname);
                DIYLog.Log("加载SpriteAtlas之后!");
                timg.sprite = spriteatlas.GetSprite(spritename);
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                timg.ABI = abi;
                timg.AtlasPath = atlaspath;
                timg.SpriteName = spritename;
            }
            else
            {
                timg.ABI = null;
                timg.AtlasPath = string.Empty;
                timg.SpriteName = string.Empty;
            }
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置TImage指定图片(单图的时候)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="spritepath">Sprite路径</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setTImageSingleSprite(TImage timg, string spritepath, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "setTImageSingleSprite!");
        ResourceModuleManager.Singleton.requstResource(spritepath,
        (abi) =>
        {
            // 清除老的资源引用
            if (timg.ABI != null && !string.IsNullOrEmpty(timg.AtlasPath))
            {
                timg.ABI.releaseOwner(timg);
            }
            if (abi != null)
            {
                var spritename = Path.GetFileNameWithoutExtension(spritepath);
                var sprite = abi.getAsset<Sprite>(timg, spritename);
                timg.sprite = sprite;
                timg.ABI = abi;
                timg.AtlasPath = spritepath;
                timg.SpriteName = spritename;
            }
            else
            {
                timg.ABI = null;
                timg.AtlasPath = string.Empty;
                timg.SpriteName = string.Empty;
            }
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="texturepath">纹理路径</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void setRawImage(TRawImage trawimg, string texturepath, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(trawimg == null, "setRawImage不允许传空TRawImage!");
        ResourceModuleManager.Singleton.requstResource(texturepath,
        (abi) =>
        {
            // 清除老的资源引用
            if (trawimg.ABI != null && !string.IsNullOrEmpty(trawimg.TextureName))
            {
                trawimg.ABI.releaseOwner(trawimg);
            }
            var assetname = Path.GetFileName(texturepath);
            if (abi != null)
            {
                var texture = abi.getAsset<Texture>(trawimg, assetname);
                trawimg.texture = texture;
            }
            trawimg.ABI = abi;
            trawimg.TextureName = texturepath;
        },
        loadtype,
        loadmethod);
    }
}