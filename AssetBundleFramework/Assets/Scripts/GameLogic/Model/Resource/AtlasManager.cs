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
    //    // TODO:待填坑
    //}

    /// <summary>
    /// 加载指定图集
    /// Note:
    /// 只加载AB不加载Sprite且不添加计数和绑定
    /// 一般用于加载常驻图集
    /// </summary>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadType">资源加载类型</param>
    public void LoadAtlas(string atlasName, out AssetLoader assetLoader,
                          Action<SpriteAtlas, AssetRequestHandle> callBack = null,
                          ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        ResourceModuleManager.Singleton.RequstAssetSync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"LoadAtlas加载atlasName:{atlasName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                var spriteAtlas = loader.GetAsset<SpriteAtlas>();
                callBack?.Invoke(spriteAtlas, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 异步加载指定图集
    /// Note:
    /// 只加载AB不加载Sprite且不添加计数和绑定
    /// 一般用于加载常驻图集
    /// </summary>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callBack">资源回调</param>
    /// <param name="loadType">资源加载类型</param>
    public AssetRequestHandle LoadAtlasAsync(string atlasName, AssetLoader assetLoader,
                                             Action<SpriteAtlas, AssetRequestHandle> callBack = null,
                                             ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        return ResourceModuleManager.Singleton.RequstAssetAsync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"LoadAtlasAsync异步加载atlasName:{atlasName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                var spriteAtlas = loader.GetAsset<SpriteAtlas>();
                callBack?.Invoke(spriteAtlas, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public void SetImageSingleSprite(Image img, string spriteName,
                                     Action<Sprite, AssetRequestHandle> callBack = null,
                                     ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(img == null, "SetImageSingleSprite不允许传空Image!");
        AssetLoader assetLoader;
        ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetImageSingleSprite加载spriteName:{spriteName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                var sprite = loader.BindAsset<Sprite>(img);
                img.sprite = sprite;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 异步设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="assetLoader">Assset加载器</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetImageSingleSpriteAsync(Image img, string spriteName,
                                                        out AssetLoader assetLoader,
                                                        Action<Sprite, AssetRequestHandle> callBack = null,
                                                        ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(img == null, "setImageSingleSprite不允许传空Image!");
        return ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetImageSingleSpriteAsync异步加载Sprite:{spriteName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                var sprite = loader.BindAsset<Sprite>(img);
                img.sprite = sprite;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 设置TImage指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="spriteName">Sprite名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSingleSprite(TImage timg, string spriteName,
                                                    Action<Sprite, AssetRequestHandle> callBack = null,
                                                    ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setTImageSingleSprite!");
        AssetLoader assetLoader;
        return ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSingleSprite加载Sprite:{spriteName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var sprite = loader.BindAsset<Sprite>(timg);
                timg.sprite = sprite;
                timg.Loader = loader;
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
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
                                                         out AssetLoader assetLoader,
                                                         Action<Sprite, AssetRequestHandle> callBack = null,
                                                         ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "SetTImageSingleSpriteAsync!");
        return ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
            spriteName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSingleSpriteAsync异步加载Sprite:{spriteName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var sprite = loader.BindAsset<Sprite>(timg);
                timg.sprite = sprite;
                timg.Loader = loader;
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
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
        return ResourceModuleManager.Singleton.RequstAssetSync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSpriteAtlas加载SpriteAtlas:{atlasName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var atlasname = Path.GetFileNameWithoutExtension(atlasName);
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = loader.BindAsset<SpriteAtlas>(timg);
                DIYLog.Log("加载SpriteAtlas之后!");
                var sprite = spriteatlas.GetSprite(spriteName);
                timg.sprite = sprite;
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                timg.Loader = loader;
                timg.SpritePath = Path.Combine(atlasName, spriteName);
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 异步设置TImage指定图片(通过先加载SpriteAtlas再加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlasName">图集名(含后缀)</param>
    /// <param name="spriteName">Sprite名(不含后缀)</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="callBack">回调</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSpriteAtlasAsync(TImage timg, string atlasName, string spriteName,
                                                        out AssetLoader assetLoader,
                                                        Action<Sprite, AssetRequestHandle> callBack = null,
                                                        ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        return ResourceModuleManager.Singleton.RequstAssetAsync<SpriteAtlas>(
            atlasName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSpriteAtlasAsync异步加载SpriteAtlas:{atlasName} AB完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var atlasname = Path.GetFileNameWithoutExtension(atlasName);
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = loader.BindAsset<SpriteAtlas>(timg);
                DIYLog.Log("加载SpriteAtlas之后!");
                var sprite = spriteatlas.GetSprite(spriteName);
                timg.sprite = sprite;
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                timg.Loader = loader;
                timg.SpritePath = Path.Combine(atlasName, spriteName);
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
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
        return ResourceModuleManager.Singleton.RequstAssetSync<Texture2D>(
            multipleTextureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSubSprite加载MultipleTexture:{multipleTextureName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var sprite = loader.BindSubAsset<Sprite>(spriteName, timg);
                timg.sprite = sprite;
                timg.Loader = loader;
                // SubAsset默认计数和对象绑定都是绑在主Asset上的，所以记录主Asset的路径
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 异步设置TImage指定图片(通过Multiple Sprite加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="multipleTextureName">MultipleTexture图路径</param>
    /// <param name="spriteName">Sprite名(不含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetTImageSubSpriteAsync(TImage timg, string multipleTextureName, string spriteName,
                                                      out AssetLoader assetLoader,
                                                      Action<Sprite, AssetRequestHandle> callBack = null,
                                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "SetTImageSubSpriteAsync不允许传空TImage!");
        return ResourceModuleManager.Singleton.RequstAssetAsync<Texture2D>(
            multipleTextureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetTImageSubSpriteAsync异步加载MultipleTetxure:{multipleTextureName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var sprite = loader.BindSubAsset<Sprite>(spriteName, timg);
                timg.sprite = sprite;
                timg.Loader = loader;
                // SubAsset默认计数和对象绑定都是绑在主Asset上的，所以记录主Asset的路径
                timg.SpritePath = loader.ResourcePath;
                callBack?.Invoke(sprite, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="textureName">纹理名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public void SetRawImage(TRawImage trawimg, string textureName,
                            Action<Texture, AssetRequestHandle> callBack = null,
                            ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(trawimg == null, "SetRawImage不允许传空TRawImage!");
        AssetLoader assetLoader;
        ResourceModuleManager.Singleton.RequstAssetSync<Texture>(
            textureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetRawImage加载textureName:{textureName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (trawimg.Loader != null && !string.IsNullOrEmpty(trawimg.TexturePath))
                {
                    trawimg.Loader.ReleaseOwner(trawimg);
                }
                var texture = loader.BindAsset<Texture>(trawimg);
                trawimg.texture = texture;
                trawimg.Loader = loader;
                trawimg.TexturePath = loader.ResourcePath;
                callBack?.Invoke(texture, assetRequestHandle);
            },
            loadType
        );
    }

    /// <summary>
    /// 异步设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="textureName">纹理名(含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle SetRawImageAsync(TRawImage trawimg, string textureName,
                                               out AssetLoader assetLoader,
                                               Action<Texture, AssetRequestHandle> callBack = null,
                                               ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(trawimg == null, "SetRawImageAsync不允许传空TRawImage!");
        return ResourceModuleManager.Singleton.RequstAssetAsync<Texture>(
            textureName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"SetRawImageAsync异步加载textureName:{textureName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }

                // 清除老的资源引用
                if (trawimg.Loader != null && !string.IsNullOrEmpty(trawimg.TexturePath))
                {
                    trawimg.Loader.ReleaseOwner(trawimg);
                }
                var texture = loader.BindAsset<Texture>(trawimg);
                trawimg.texture = texture;
                trawimg.Loader = loader;
                trawimg.TexturePath = loader.ResourcePath;
                callBack?.Invoke(texture, assetRequestHandle);
            },
            loadType
        );
    }
}
