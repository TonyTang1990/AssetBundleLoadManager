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
    //    // TODO:待填坑
    //}

    /// <summary>
    /// 加载指定图集
    /// Note:
    /// 只加载AB不加载Sprite且不添加计数和绑定
    /// 一般用于加载常驻图集
    /// </summary>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    public void loadAtlas(string atlasPath, Action<int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.BundleLoader assetBundleLoader;
        TResource.ResourceModuleManager.Singleton.RequstAssetBundleSync(
            atlasPath,
            out assetBundleLoader,
            (loader, requestUid) =>
            {
                var bundle = loader?.ObtainAssetBundle();
                callback?.Invoke(requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步加载指定图集
    /// Note:
    /// 只加载AB不加载Sprite且不添加计数和绑定
    /// 一般用于加载常驻图集
    /// </summary>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="bundleLoader">bundle加载器</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    public int loadAtlasAsync(string atlaspath, out TResource.BundleLoader bundleLoader, Action<int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        return TResource.ResourceModuleManager.Singleton.RequstAssetBundleAsync(
            atlaspath,
            out bundleLoader,
            (loader, requestUid) =>
            {
                var bundle = loader?.ObtainAssetBundle();
                callback?.Invoke(requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spritePath">Sprite路径</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public void setImageSingleSprite(Image img, string spritePath, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(img == null, "setImageSingleSprite不允许传空Image!");
        TResource.AssetLoader assetLoader;
        TResource.ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
            spritePath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var sprite = loader.BindAsset<Sprite>(img);
                img.sprite = sprite;
                callback?.Invoke(sprite, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步设置Image指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="img">Image组件</param>
    /// <param name="spritePath">Sprite路径</param>
    /// <param name="assetLoader">Assset加载器</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setImageSingleSpriteAsync(Image img, string spritePath, out TResource.AssetLoader assetLoader, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(img == null, "setImageSingleSprite不允许传空Image!");
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
            spritePath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var sprite = loader.BindAsset<Sprite>(img);
                img.sprite = sprite;
                callback?.Invoke(sprite, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 设置TImage指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="spritepath">Sprite路径</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setTImageSingleSprite(TImage timg, string spritepath, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setTImageSingleSprite!");
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
            spritepath,
            out assetLoader,
            (loader, requestUid) =>
            {
                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var sprite = loader.BindAsset<Sprite>(timg);
                timg.sprite = sprite;
                timg.Loader = loader;
                timg.SpritePath = spritepath;
                callback?.Invoke(sprite, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步设置TImage指定图片(单图或者SpriteAtlas里的图)
    /// </summary>
    /// <param name="timg">TImage组件</param>
    /// <param name="callback">回调</param>
    /// <param name="spritepath">Sprite路径</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setTImageSingleSpriteAsync(TImage timg, string spritepath, out TResource.AssetLoader assetLoader, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setTImageSingleSprite!");
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
            spritepath,
            out assetLoader,
            (loader, requestUid) =>
            {
                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var sprite = loader.BindAsset<Sprite>(timg);
                timg.sprite = sprite;
                timg.Loader = loader;
                timg.SpritePath = spritepath;
                callback?.Invoke(sprite, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 设置TImage指定图片(通过先加载SpriteAtlas再加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="spritename">Sprite名</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setTImageSpriteAtlas(TImage timg, string atlaspath, string spritename, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<SpriteAtlas>(
            atlaspath,
            out assetLoader,
            (loader, requestUid) =>
            {
                DIYLog.Log($"加载SpriteAtlas:{atlaspath}完成!");
                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var atlasname = Path.GetFileNameWithoutExtension(atlaspath);
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = loader.BindAsset<SpriteAtlas>(timg);
                DIYLog.Log("加载SpriteAtlas之后!");
                var sprite = spriteatlas.GetSprite(spritename);
                timg.sprite = sprite;
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                timg.Loader = loader;
                timg.SpritePath = Path.Combine(atlaspath, spritename);
                callback?.Invoke(sprite, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步设置TImage指定图片(通过先加载SpriteAtlas再加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="atlaspath">图集路径</param>
    /// <param name="spritename">Sprite名</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="callback">回调</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <returns></returns>
    public int setTImageSpriteAtlasAsync(TImage timg, string atlaspath, string spritename, out TResource.AssetLoader assetLoader, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(timg == null, "setImageSpriteAtlas不允许传空TImage!");
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<SpriteAtlas>(
            atlaspath,
            out assetLoader,
            (loader, requestUid) =>
            {
                DIYLog.Log($"加载SpriteAtlas:{atlaspath} AB完成!");
                // 清除老的资源引用
                if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
                {
                    timg.Loader.ReleaseOwner(timg);
                    timg.Loader = null;
                }
                var atlasname = Path.GetFileNameWithoutExtension(atlaspath);
                DIYLog.Log("加载SpriteAtlas之前!");
                var spriteatlas = loader.BindAsset<SpriteAtlas>(timg);
                DIYLog.Log("加载SpriteAtlas之后!");
                var sprite = spriteatlas.GetSprite(spritename);
                timg.sprite = sprite;
                DIYLog.Log("SpriteAtlas.GetSprite()之后!");
                timg.Loader = loader;
                timg.SpritePath = Path.Combine(atlaspath, spritename);
                callback?.Invoke(sprite, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 设置TImage指定图片(通过Multiple Sprite加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="multipleSpritePath">MultipleSprite图路径</param>
    /// <param name="spritename">Sprite名</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setTImageSubSprite(TImage timg, string multipleSpritePath, string spritename, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        // TODO:
        // 改造成支持SubAsset的加载方式
        // 目前设计思路是定义一个AssetLoadType区分是常规Asset加载还是SubAsset加载
        // 底层AssetInfo里添加AssetLoadType和SubAssetName字段
        // 上层统一封装AssetLoadType的传参
        // 最后底层加载完成后根据AssetLaodType和SubAssetName决定是LoadAsset还是LoadSubAsset
        DIYLog.LogError($"暂未支持SubAsset的加载方式，请勿设计SubAsset的资源直接使用!");
        return -1;
        // DIYLog.Assert(timg == null, "setTImageSubSprite不允许传空TImage!");
        // TResource.AssetLoader assetLoader;
        // return TResource.ResourceModuleManager.Singleton.RequstAssetSync<Sprite>(
        //     multipleSpritePath,
        //     out assetLoader,
        //     (loader, requestUid) =>
        //     {
        //         DIYLog.Log($"加载MultipleSprite:{multipleSpritePath}完成!");
        //         // 清除老的资源引用
        //         if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
        //         {
        //             timg.Loader.ReleaseOwner(timg);
        //             timg.Loader = null;
        //         }
        //         var sprite = loader.BindAsset<Sprite>(timg);
        //         timg.sprite = sprite;
        //         timg.Loader = loader;
        //         timg.SpritePath = multipleSpritePath;
        //         callback?.Invoke(sprite, requestUid);
        //     },
        //     loadtype
        // );
    }

    /// <summary>
    /// 异步设置TImage指定图片(通过Multiple Sprite加载Sprite的方式)
    /// </summary>
    /// <param name="timg">Image组件</param>
    /// <param name="multipleSpritePath">MultipleSprite图路径</param>
    /// <param name="spritename">Sprite名</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setTImageSubSpriteAsync(TImage timg, string multipleSpritePath, string spritename, out TResource.AssetLoader assetLoader, Action<Sprite, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        // TODO:
        // 改造成支持SubAsset的加载方式
        // 目前设计思路是定义一个AssetLoadType区分是常规Asset加载还是SubAsset加载
        // 底层AssetInfo里添加AssetLoadType和SubAssetName字段
        // 上层统一封装AssetLoadType的传参
        // 最后底层加载完成后根据AssetLaodType和SubAssetName决定是LoadAsset还是LoadSubAsset
        DIYLog.LogError($"暂未支持SubAsset的异步加载方式，请勿设计SubAsset的资源直接使用!");
        assetLoader = null;
        return -1;
        // DIYLog.Assert(timg == null, "setTImageSubSpriteAsync不允许传空TImage!");
        // return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<Sprite>(
        //     multipleSpritePath,
        //     out assetLoader,
        //     (loader, requestUid) =>
        //     {
        //         DIYLog.Log($"加载MultipleSprite:{multipleSpritePath}完成!");
        //         // 清除老的资源引用
        //         if (timg.Loader != null && !string.IsNullOrEmpty(timg.SpritePath))
        //         {
        //             timg.Loader.ReleaseOwner(timg);
        //             timg.Loader = null;
        //         }
        //         var sprite = loader.BindAsset<Sprite>(timg);
        //         timg.sprite = sprite;
        //         timg.Loader = loader;
        //         timg.SpritePath = multipleSpritePath;
        //         callback?.Invoke(sprite, requestUid);
        //     },
        //     loadtype
        // );
    }

    /// <summary>
    /// 设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="texturepath">纹理路径</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public void setRawImage(TRawImage trawimg, string texturepath, Action<Texture, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(trawimg == null, "setRawImage不允许传空TRawImage!");
        TResource.AssetLoader assetLoader;
        TResource.ResourceModuleManager.Singleton.RequstAssetSync<Texture>(
            texturepath,
            out assetLoader,
            (loader, requestUid) =>
            {
                // 清除老的资源引用
                if (trawimg.Loader != null && !string.IsNullOrEmpty(trawimg.TextureName))
                {
                    trawimg.Loader.ReleaseOwner(trawimg);
                }
                var texture = loader.BindAsset<Texture>(trawimg);
                trawimg.texture = texture;
                trawimg.Loader = loader;
                trawimg.TextureName = texturepath;
                callback?.Invoke(texture, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步设置Image指定图片
    /// </summary>
    /// <param name="trawimg">Image组件</param>
    /// <param name="texturepath">纹理路径</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callback">回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int setRawImageAsync(TRawImage trawimg, string texturepath, out TResource.AssetLoader assetLoader, Action<Texture, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        DIYLog.Assert(trawimg == null, "setRawImage不允许传空TRawImage!");
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<Texture>(
            texturepath,
            out assetLoader,
            (loader, requestUid) =>
            {
                // 清除老的资源引用
                if (trawimg.Loader != null && !string.IsNullOrEmpty(trawimg.TextureName))
                {
                    trawimg.Loader.ReleaseOwner(trawimg);
                }
                var texture = loader.BindAsset<Texture>(trawimg);
                trawimg.texture = texture;
                trawimg.Loader = loader;
                trawimg.TextureName = texturepath;
                callback?.Invoke(texture, requestUid);
            },
            loadtype
        );
    }
}