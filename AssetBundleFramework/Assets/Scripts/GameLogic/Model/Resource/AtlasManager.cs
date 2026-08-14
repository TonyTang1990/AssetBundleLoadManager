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
    /// <param name="callback">资源回调</param>
    /// <param name="loadType">资源加载类型</param>
    public void LoadAtlas(string atlasName, ResourceScope resourceScope,
                          Action<SpriteAtlas, AssetRequestHandle> callBack = null,
                          ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"LoadAtlas加载atlasName:{atlasName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                var spriteAtlas = resourceScope.GetAsset<SpriteAtlas>(loader);
                callBack?.Invoke(spriteAtlas, assetRequestHandle);
            },
            loadType
        );
        resourceScope.RecordRequest(assetRequestHandle);
    }

    /// <summary>
    /// 异步加载指定图集
    /// Note:
    /// 只加载AB不加载Sprite且不添加计数和绑定
    /// 一般用于加载常驻图集
    /// </summary>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">资源回调</param>
    /// <param name="loadType">资源加载类型</param>
    public AssetRequestHandle LoadAtlasAsync(string atlasName, ResourceScope resourceScope,
                                             Action<SpriteAtlas, AssetRequestHandle> callBack = null,
                                             ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"LoadAtlasAsync异步加载atlasName:{atlasName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                var spriteAtlas = resourceScope.GetAsset<SpriteAtlas>(loader);
                callBack?.Invoke(spriteAtlas, assetRequestHandle);
            },
            loadType
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public void SetImageSingleSprite(Image img, string spriteName,
                                     ResourceScope resourceScope,
                                     Action<Sprite, AssetRequestHandle> callBack = null,
                                     ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(img == null, "SetImageSingleSprite不允许传空Image!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetImageSingleSprite加载spriteName:{spriteName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                var sprite = resourceScope.GetAsset<Sprite>(loader);
                img.sprite = sprite;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
        resourceScope.RecordRequest(assetRequestHandle);
    }

    /// <summary>
    /// 异步设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="resourceScope">资源资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetImageSingleSpriteAsync(Image img, string spriteName,
                                                        ResourceScope resourceScope,
                                                        Action<Sprite, AssetRequestHandle> callBack = null,
                                                        ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(img == null, "setImageSingleSprite不允许传空Image!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetImageSingleSpriteAsync异步加载Sprite:{spriteName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                var sprite = resourceScope.GetAsset<Sprite>(loader);
                img.sprite = sprite;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 设置TImage指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="resourceScope">资源资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSingleSprite(TImage timg, string spriteName,
                                                    Action<Sprite, AssetRequestHandle> callBack = null,
                                                    ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setTImageSingleSprite!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSingleSprite加载Sprite:{spriteName}完成!");
                timg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                timg.ReleaseSpriteRes();
                var sprite = timg.ResourceScope.GetAsset<Sprite>(loader);
                timg.sprite = sprite;
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
        timg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步设置TImage指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="callBack">回调</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSingleSpriteAsync(TImage timg, string spriteName,
                                                         Action<Sprite, AssetRequestHandle> callBack = null,
                                                         ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "SetTImageSingleSpriteAsync!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSingleSpriteAsync异步加载Sprite:{spriteName}完成!");
                timg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                timg.ReleaseSpriteRes();
                var sprite = timg.ResourceScope.GetAsset<Sprite>(loader);
                timg.sprite = sprite;
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
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
    /// <returns></returns>
    public AssetRequestHandle SetTImageSpriteAtlas(TImage timg, string atlasName, string spriteName,
                                                   Action<Sprite, AssetRequestHandle> callBack = null,
                                                   ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSpriteAtlas加载SpriteAtlas:{atlasName}完成!");
                timg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                timg.ReleaseSpriteRes();
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = timg.ResourceScope.GetAsset<SpriteAtlas>(loader);
                DIYLog.Log("加载SpriteAtlas之后!");
                var sprite = spriteatlas.GetSprite(spriteName);
                timg.sprite = sprite;
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                // 计数是加载SpriteAtlas身上的
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
        timg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步设置TImage指定图片(通过先加载SpriteAtlas再加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="spriteName">Sprite名(不含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSpriteAtlasAsync(TImage timg, string atlasName, string spriteName,
                                                        Action<Sprite, AssetRequestHandle> callBack = null,
                                                        ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSpriteAtlasAsync异步加载SpriteAtlas:{atlasName} AB完成!");
                timg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                timg.ReleaseSpriteRes();
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = timg.ResourceScope.GetAsset<SpriteAtlas>(loader);
                DIYLog.Log("加载SpriteAtlas之后!");
                var sprite = spriteatlas.GetSprite(spriteName);
                timg.sprite = sprite;
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                // 计数是加载SpriteAtlas身上的
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
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
    /// <returns></returns>
    public AssetRequestHandle SetTImageSubSprite(TImage timg, string multipleTextureName, string spriteName,
                                                 Action<Sprite, AssetRequestHandle> callBack = null,
                                                 ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "SetTImageSubSprite不允许传空TImage!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<Texture2D>(
            multipleTextureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSubSprite加载MultipleTexture:{multipleTextureName}完成!");
                timg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                timg.ReleaseSpriteRes();
                var sprite = timg.ResourceScope.GetSubAsset<Sprite>(loader, spriteName);
                timg.sprite = sprite;
                // SubAsset默认计数和对象绑定都是绑在主Asset上的，所以记录主Asset的路径
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
        timg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步设置TImage指定图片(通过Multiple Sprite加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="multipleTextureName">MultipleTexture图路径</param>
    /// <param name="spriteName">Sprite名(不含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSubSpriteAsync(TImage timg, string multipleTextureName, string spriteName,
                                                      Action<Sprite, AssetRequestHandle> callBack = null,
                                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "SetTImageSubSpriteAsync不允许传空TImage!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<Texture2D>(
            multipleTextureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSubSpriteAsync异步加载MultipleTetxure:{multipleTextureName}完成!");
                timg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                timg.ReleaseSpriteRes();
                var sprite = timg.ResourceScope.GetSubAsset<Sprite>(loader, spriteName);
                timg.sprite = sprite;
                // SubAsset默认计数和对象绑定都是绑在主Asset上的，所以记录主Asset的路径
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
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
    /// <returns></returns>
    public AssetRequestHandle SetRawImage(TRawImage trawimg, string textureName,
                                          Action<Texture, AssetRequestHandle> callBack = null,
                                          ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(trawimg == null, "SetRawImage不允许传空TRawImage!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<Texture>(
            textureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetRawImage加载textureName:{textureName}完成!");
                trawimg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                trawimg.ReleaseTextureRes();
                var texture = trawimg.ResourceScope.GetAsset<Texture>(loader);
                trawimg.texture = texture;
                trawimg.TexturePath = loader.ResourcePath;
                callBack?.Invoke(texture, assetRequestHandle);
            },
            loadType
        );
        trawimg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="textureName">纹理名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetRawImageAsync(TRawImage trawimg, string textureName,
                                               Action<Texture, AssetRequestHandle> callBack = null,
                                               ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(trawimg == null, "SetRawImageAsync不允许传空TRawImage!");
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<Texture>(
            textureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetRawImageAsync异步加载textureName:{textureName}完成!");
                trawimg.ResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                trawimg.ReleaseTextureRes();
                var texture = trawimg.ResourceScope.GetAsset<Texture>(loader);
                trawimg.texture = texture;
                trawimg.TexturePath = loader.ResourcePath;
                callBack?.Invoke(texture, assetRequestHandle);
            },
            loadType
        );
        trawimg.ResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }
}
