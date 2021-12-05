/*
 * Description:             AudioManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// AudioManager.cs
/// 音效单例管理类
/// </summary>
public class AudioManager : SingletonTemplate<AudioManager>
{
    /// <summary>
    /// 音效播放信息
    /// </summary>
    public class SFXAudioInfo : IRecycle
    {
#if !NEW_RESOURCE
        /// <summary>
        /// 资源加载信息
        /// </summary>
        public AbstractResourceInfo ABI
        {
            get;
            set;
        }
#else
        /// <summary>
        /// Asset加载器
        /// </summary>
        public TResource.AssetLoader Loader
        {
            get;
            set;
        }
#endif

        /// <summary>
        /// 音效绑定对象
        /// </summary>
        public GameObject SFXAudioGo
        {
            get;
            set;
        }

        /// <summary>
        /// 音效组件
        /// </summary>
        public AudioSource SFXAudioSource
        {
            get;
            set;
        }

        public void onCreate()
        {

        }

        public void onDispose()
        {
#if !NEW_RESOURCE
            ABI = null;
#else
            Loader = null;
#endif
            SFXAudioGo = null;
            SFXAudioSource = null;
        }
    }

    /// <summary>
    /// 音效资源模板名
    /// </summary>
    private const string AudioGoResName = "SFXTemplate";
    
    /// <summary>
    /// 音效GameObject对象池
    /// </summary>
    private GameObjectPool mAudioGoPool;

    /// <summary>
    /// 音效实体对象模板
    /// </summary>
    private GameObject mSFXGoTemplate;

    /// <summary>
    /// 音效实体对象模板InstanceID
    /// </summary>
    private int mSFXInstanceID;

    /// <summary>
    /// 背景音效组件
    /// </summary>
    private AudioSource mBGMAudioSource;

#if !NEW_RESOURCE
    /// <summary>
    /// 当前场景背景音乐的资源信息
    /// </summary>
    private AbstractResourceInfo mCurrentBGMARI;
#else
    /// <summary>
    /// 当前背景音乐的Asset加载器
    /// </summary>
    private TResource.AssetLoader mCurrentBGMAssetLoader;
#endif

    public AudioManager()
    {
        mAudioGoPool = new GameObjectPool("AudioGoPool");
        mSFXGoTemplate = new GameObject("SfxAudio");
        mSFXInstanceID = mSFXGoTemplate.GetInstanceID();
        mSFXGoTemplate.AddComponent<AudioSource>();
        mAudioGoPool.Init(mSFXGoTemplate, 5);
        var bgmgo = new GameObject("BGMAudio");
        UnityEngine.Object.DontDestroyOnLoad(bgmgo);
        mBGMAudioSource = bgmgo.AddComponent<AudioSource>();

        ObjectPool.Singleton.initialize<SFXAudioInfo>(5);
    }

#if !NEW_RESOURCE
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="respath">资源路径</param>
    public void playSFXSound(string respath)
    {
        var sfxgo = mAudioGoPool.Pop(mSFXGoTemplate);
        ResourceModuleManager.Singleton.requstResource(respath,
        (abi) =>
        {
            var sfxaudioinfo = ObjectPool.Singleton.pop<SFXAudioInfo>();
            var sfxname = Path.GetFileName(respath);
            var ac = abi.getAsset<AudioClip>(sfxgo, sfxname);
            var audiosource = sfxgo.GetComponent<AudioSource>();
            sfxaudioinfo.SFXAudioGo = sfxgo;
            sfxaudioinfo.SFXAudioSource = audiosource;
            sfxaudioinfo.ABI = abi;
            audiosource.clip = ac;
            audiosource.Play();
            Timer.Singleton.addTimer(() =>
            {
                // 手动释放音效资源绑定，因为音效绑定对象会进池会导致无法满足释放条件
                sfxaudioinfo.SFXAudioSource.clip = null;
                sfxaudioinfo.ABI.releaseOwner(sfxaudioinfo.SFXAudioGo);
                mAudioGoPool.Push(mSFXInstanceID, sfxaudioinfo.SFXAudioGo);
                ObjectPool.Singleton.push<SFXAudioInfo>(sfxaudioinfo);
            }, ac.length);
        });
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="loop">是否循环播放</param>
    public void playBGM(string respath, bool loop = true)
    {
        //背景音效是挂载DontDestroyOnLoad上会导致永远无法满足卸载条件，所以需要手动移除对象绑定
        if (mCurrentBGMARI != null)
        {
            mCurrentBGMARI.releaseOwner(mBGMAudioSource);
        }

        var assetname = Path.GetFileName(respath);
        ResourceModuleManager.Singleton.requstResource(respath,
        (ari) =>
        {
            mCurrentBGMARI = ari;
            var clip = ari.getAsset<AudioClip>(mBGMAudioSource, assetname);
            mBGMAudioSource.clip = clip;
            mBGMAudioSource.loop = loop;
            mBGMAudioSource.Play();
        });
    }
#else
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callback">回调</param>
    /// <param name="loadType">加载类型</param>
    /// <returns></returns>
    public int playSFXSound(string respath, out TResource.AssetLoader assetLoader, Action<AudioClip, int> callback = null, TResource.ResourceLoadType loadType = TResource.ResourceLoadType.NormalLoad)
    {
        var sfxgo = mAudioGoPool.Pop(mSFXGoTemplate);
        return TResource.ResourceModuleManager.Singleton.requstAssetSync<AudioClip>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var sfxaudioinfo = ObjectPool.Singleton.pop<SFXAudioInfo>();
                var ac = loader.bindAsset<AudioClip>(sfxgo);
                var audiosource = sfxgo.GetComponent<AudioSource>();
                sfxaudioinfo.SFXAudioGo = sfxgo;
                sfxaudioinfo.SFXAudioSource = audiosource;
                sfxaudioinfo.Loader = loader;
                audiosource.clip = ac;
                audiosource.Play();
                Timer.Singleton.addTimer(() =>
                {
                    // 手动释放音效资源绑定，因为音效绑定对象会进池会导致无法满足释放条件
                    sfxaudioinfo.SFXAudioSource.clip = null;
                    sfxaudioinfo.Loader.releaseOwner(sfxaudioinfo.SFXAudioGo);
                    mAudioGoPool.Push(mSFXInstanceID, sfxaudioinfo.SFXAudioGo);
                    ObjectPool.Singleton.push<SFXAudioInfo>(sfxaudioinfo);
                }, ac.length);
                callback?.Invoke(ac, requestUid);
            },
            loadType
        );
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="callback">回调</param>
    /// <param name="loadType">加载类型</param>
    /// <returns></returns>
    public int playBGM(string respath, out TResource.AssetLoader assetLoader, bool loop = true, Action<AudioClip, int> callback = null, TResource.ResourceLoadType loadType = TResource.ResourceLoadType.NormalLoad)
    {
        //背景音效是挂载DontDestroyOnLoad上会导致永远无法满足卸载条件，所以需要手动移除对象绑定
        if (mCurrentBGMAssetLoader != null)
        {
            mCurrentBGMAssetLoader.releaseOwner(mBGMAudioSource);
            mCurrentBGMAssetLoader = null;
        }

        return TResource.ResourceModuleManager.Singleton.requstAssetSync<AudioClip>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                mCurrentBGMAssetLoader = loader;
                var clip = loader.bindAsset<AudioClip>(mBGMAudioSource);
                mBGMAudioSource.clip = clip;
                mBGMAudioSource.loop = loop;
                mBGMAudioSource.Play();
                callback?.Invoke(clip, requestUid);
            },
            loadType
        );
    }
#endif

}