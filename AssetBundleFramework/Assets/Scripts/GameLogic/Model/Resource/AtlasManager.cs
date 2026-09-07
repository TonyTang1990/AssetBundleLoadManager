/*
 * Description:             AtlasManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.IO;
using TResource;
using TUI;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// AtlasManager.cs
/// 图集管理单例类
/// </summary>
public class AtlasManager : SingletonBase<AtlasManager>
{
    public AtlasManager()
    {
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        //DIYLog.Log("添加SpriteAtals图集延时绑定回调!");
        //SpriteAtlasManager.atlasRequested += OnAtlasRequested;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public override void Shutdown()
    {
        base.Shutdown();
        //DIYLog.Log("移除SpriteAtals图集延时绑定回调!");
        //SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
    }

    ///// <summary>
    ///// 响应SpriteAtlas图集加载回调
    ///// </summary>
    ///// <param name="atlaspath"></param>
    ///// <param name="callback"></param>
    //private void OnAtlasRequested(string atlaspath, Action<SpriteAtlas> callback)
    //{
    //    DIYLog.Log($"加载SpriteAtlas:{atlaspath}");
    //    // Later Bind -- 依赖使用SpriteAtlas的加载都会触发这里
    //    // TODO:待填坑
    //}

    /// <summary>
    /// 加载指定图集(加计数)
    /// Note:
    /// 一般用于加载常驻图集
    /// </summary>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">资源回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle LoadAtlasAsync(string atlasName, ResourceScope resourceScope,
                                             Action<SpriteAtlas, AssetRequestHandle> callBack = null,
                                             ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                             ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return ResourceManager.Singleton.GetAssetByCb(atlasName, callBack, resourceScope,
                                                            loadType, loadMethod);
    }

    /// <summary>
    /// 设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle SetImageSingleSpriteAsync(Image img, string spriteName,
                                                        ResourceScope resourceScope,
                                                        Action<Sprite, AssetRequestHandle> callBack = null,
                                                        ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                        ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(img == null, "setImageSingleSprite不允许传空Image!");
        return ResourceManager.Singleton.GetAssetByCb<Sprite>(spriteName, (sprite, assetRequestHandle) =>
        {
            if (sprite != null)
            {
                img.sprite = sprite;
            }
            callBack?.Invoke(sprite, assetRequestHandle);
        }, resourceScope, loadType, loadMethod);
    }

    /// <summary>
    /// 设置TImage指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle SetTImageSingleSpriteAsync(TImage timg, string spriteName,
                                                         Action<Sprite, AssetRequestHandle> callBack = null,
                                                         ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                         ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "SetTImageSingleSpriteAsync!");
        AssetLoader assetLoader;
        Action<AssetLoader, AssetRequestHandle> completeHandler = (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"SetTImageSingleSpriteAsync加载Sprite:{spriteName}完成!");
            timg.ResourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsSuccess)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var sprite = timg.ResourceScope.GetAsset<Sprite>(loader);
            if(sprite == null)
            {
                Debug.LogError($"SetTImageSingleSpriteAsync加载Sprite:{spriteName}失败，Sprite为null!");
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            // 清除老的资源引用
            timg.ReleaseSpriteRes();
            timg.sprite = sprite;
            timg.SpritePath = loader.ResourcePath;
            callBack?.Invoke(sprite, assetRequestHandle);
        };
        var assetRequestHandle = ResourceManager.Singleton.RequestAssetAsync<Sprite>(spriteName, out assetLoader,
                                                                                     completeHandler, loadType,
                                                                                     loadMethod);
        timg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 设置TImage指定图片(通过先加载SpriteAtlas再加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="spriteName">Sprite名(不含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle SetTImageSpriteAtlasAsync(TImage timg, string atlasName, string spriteName,
                                                        Action<Sprite, AssetRequestHandle> callBack = null,
                                                        ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                        ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        AssetLoader assetLoader;
        Action<AssetLoader, AssetRequestHandle> completeHandler = (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"SetTImageSpriteAtlasAsync加载SpriteAtlas:{atlasName}完成!");
            timg.ResourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsSuccess)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var spriteAtlas = timg.ResourceScope.GetAsset<SpriteAtlas>(loader);
            if(spriteAtlas == null)
            {
                DIYLog.LogError($"加载SpriteAtlas失败:{atlasName}");
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var sprite = spriteAtlas.GetSprite(spriteName);
            if(sprite == null)
            {
                Debug.LogError($"SetTImageSpriteAtlasAsync加载Sprite:{spriteName}失败，Sprite为null!");
                // 返还spriteAtlas添加的引用
                timg.ResourceScope.ReleaseResource(loader.ResourcePath);
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            // 清除老的资源引用
            timg.ReleaseSpriteRes();
            timg.sprite = sprite;
            DIYLog.Log("SpriteAtlas.GetSprite()之后!");
            // 计数是加载SpriteAtlas身上的
            timg.SpritePath = loader.ResourcePath;
            callBack?.Invoke(sprite, assetRequestHandle);
        };
        var assetRequestHandle = ResourceManager.Singleton.RequestAssetAsync<SpriteAtlas>(atlasName, out assetLoader,
                                                                                          completeHandler, loadType,
                                                                                          loadMethod);
        timg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 设置TImage指定图片(通过Multiple Sprite加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="multipleTextureName">MultipleTexture名(含后缀)</param>
    /// <param name="spriteName">Sprite名(不含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle SetTImageSubSpriteAsync(TImage timg, string multipleTextureName, string spriteName,
                                                      Action<Sprite, AssetRequestHandle> callBack = null,
                                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                      ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(timg == null, "SetTImageSubSpriteAsync不允许传空TImage!");
        AssetLoader assetLoader;
        Action<AssetLoader, AssetRequestHandle> completeHandler = (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"SetTImageSubSpriteAsync加载MultipleTexture:{multipleTextureName}完成!");
            timg.ResourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsSuccess)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var sprite = timg.ResourceScope.GetSubAsset<Sprite>(loader, spriteName);
            if(sprite == null)
            {
                Debug.LogError($"SetTImageSubSpriteAsync加载Sprite:{spriteName}失败，Sprite为null!");
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            // 清除老的资源引用
            timg.ReleaseSpriteRes();
            timg.sprite = sprite;
            // SubAsset默认计数和对象绑定都是绑在主Asset上的，所以记录主Asset的路径
            timg.SpritePath = loader.ResourcePath;
            callBack?.Invoke(sprite, assetRequestHandle);
        };
        var assetRequestHandle = ResourceManager.Singleton.RequestAssetAsync<Texture2D>(multipleTextureName,
                                                                                        out assetLoader,
                                                                                        completeHandler, loadType,
                                                                                        loadMethod);
        timg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="textureName">纹理名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle SetRawImageAsync(TRawImage trawimg, string textureName,
                                               Action<Texture, AssetRequestHandle> callBack = null,
                                               ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                               ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        DIYLog.Assert(trawimg == null, "SetRawImageAsync不允许传空TRawImage!");
        AssetLoader assetLoader;
        Action<AssetLoader, AssetRequestHandle> completeHandler = (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"SetRawImageAsync加载textureName:{textureName}完成!");
            trawimg.ResourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsSuccess)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var texture = trawimg.ResourceScope.GetAsset<Texture>(loader);
            if(texture == null)
            {
                Debug.LogError($"SetRawImageAsync加载textureName:{textureName}失败，Texture为null!");
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            // 清除老的资源引用
            trawimg.ReleaseTextureRes();
            trawimg.texture = texture;
            trawimg.TexturePath = loader.ResourcePath;
            callBack?.Invoke(texture, assetRequestHandle);
        };
        var assetRequestHandle = ResourceManager.Singleton.RequestAssetAsync<Texture>(textureName, out assetLoader,
                                                                                      completeHandler, loadType,
                                                                                      loadMethod);
        trawimg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }
}
